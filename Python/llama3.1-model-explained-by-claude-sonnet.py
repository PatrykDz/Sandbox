# Llama 3 Model Implementation
# This code implements a state-of-the-art language model based on the transformer architecture.
# It incorporates several advanced techniques such as rotary positional embeddings,
# RMS normalization, and grouped-query attention.

import math
from typing import Optional, Tuple

import fairscale.nn.model_parallel.initialize as fs_init
import torch
import torch.nn.functional as F
from fairscale.nn.model_parallel.layers import (
    ColumnParallelLinear,
    RowParallelLinear,
    VocabParallelEmbedding,
)
from torch import nn

from .args import ModelArgs

class RMSNorm(torch.nn.Module):
    """
    Root Mean Square Layer Normalization
    
    This normalization technique is an alternative to LayerNorm, offering improved performance
    and stability. It normalizes the inputs by dividing them by the RMS of the inputs, then
    applies a learnable scale factor.

    Reference: https://arxiv.org/abs/1910.07467
    """
    def __init__(self, dim: int, eps: float = 1e-6):
        super().__init__()
        self.eps = eps
        self.weight = nn.Parameter(torch.ones(dim))

    def _norm(self, x):
        # Calculate the root mean square along the last dimension
        return x * torch.rsqrt(x.pow(2).mean(-1, keepdim=True) + self.eps)

    def forward(self, x):
        output = self._norm(x.float()).type_as(x)
        return output * self.weight

def apply_scaling(freqs: torch.Tensor):
    """
    Apply scaling to the rotary positional embeddings frequencies.
    
    This function implements a technique to extend the effective context length
    of the model by manipulating the frequency spectrum of the positional embeddings.
    It applies different scaling factors to different frequency ranges, effectively
    "stretching" the lower frequencies to cover longer contexts.

    Args:
        freqs (torch.Tensor): The original frequencies for rotary embeddings.

    Returns:
        torch.Tensor: The scaled frequencies.
    """
    scale_factor = 8
    low_freq_factor = 1
    high_freq_factor = 4
    old_context_len = 8192  # original llama3 length

    low_freq_wavelen = old_context_len / low_freq_factor
    high_freq_wavelen = old_context_len / high_freq_factor
    new_freqs = []
    for freq in freqs:
        wavelen = 2 * math.pi / freq
        if wavelen < high_freq_wavelen:
            new_freqs.append(freq)
        elif wavelen > low_freq_wavelen:
            new_freqs.append(freq / scale_factor)
        else:
            assert low_freq_wavelen != high_freq_wavelen
            smooth = (old_context_len / wavelen - low_freq_factor) / (
                high_freq_factor - low_freq_factor
            )
            new_freqs.append((1 - smooth) * freq / scale_factor + smooth * freq)
    return torch.tensor(new_freqs, dtype=freqs.dtype, device=freqs.device)

def precompute_freqs_cis(
    dim: int, end: int, theta: float = 10000.0, use_scaled: bool = False
):
    """
    Precompute the frequency tensor for complex exponentials (cos, sin) used in rotary embeddings.
    
    This function generates a tensor of complex numbers representing the rotary embeddings
    for each position and feature dimension. These embeddings allow the model to have a
    notion of relative positioning without using absolute positional encodings.

    Args:
        dim (int): The dimension of the model's hidden state.
        end (int): The maximum sequence length to precompute.
        theta (float): The base value for the angle calculation.
        use_scaled (bool): Whether to use the scaled version of frequencies.

    Returns:
        torch.Tensor: A complex tensor of shape (end, dim/2) containing the precomputed
                      frequency values.

    Reference: https://arxiv.org/abs/2104.09864
    """
    freqs = 1.0 / (theta ** (torch.arange(0, dim, 2)[: (dim // 2)].float() / dim))
    t = torch.arange(end, device=freqs.device, dtype=torch.float32)
    if use_scaled:
        freqs = apply_scaling(freqs)
    freqs = torch.outer(t, freqs)
    freqs_cis = torch.polar(torch.ones_like(freqs), freqs)  # complex64
    return freqs_cis

def reshape_for_broadcast(freqs_cis: torch.Tensor, x: torch.Tensor):
    """
    Reshape the frequency tensor for efficient broadcasting during rotary embedding application.
    
    This function prepares the precomputed frequency tensor for element-wise multiplication
    with the input tensor. It ensures that the dimensions are compatible for broadcasting.

    Args:
        freqs_cis (torch.Tensor): The precomputed frequency tensor.
        x (torch.Tensor): The input tensor to which rotary embeddings will be applied.

    Returns:
        torch.Tensor: The reshaped frequency tensor ready for broadcasting.
    """
    ndim = x.ndim
    assert 0 <= 1 < ndim
    assert freqs_cis.shape == (x.shape[1], x.shape[-1])
    shape = [d if i == 1 or i == ndim - 1 else 1 for i, d in enumerate(x.shape)]
    return freqs_cis.view(*shape)

def apply_rotary_emb(
    xq: torch.Tensor,
    xk: torch.Tensor,
    freqs_cis: torch.Tensor,
) -> Tuple[torch.Tensor, torch.Tensor]:
    """
    Apply rotary positional embeddings to the query and key tensors.
    
    This function applies the precomputed rotary embeddings to the query and key tensors
    in the attention mechanism. It allows the model to have a notion of relative positioning
    without using absolute positional encodings.

    Args:
        xq (torch.Tensor): The query tensor.
        xk (torch.Tensor): The key tensor.
        freqs_cis (torch.Tensor): The precomputed frequency tensor.

    Returns:
        Tuple[torch.Tensor, torch.Tensor]: The query and key tensors with rotary embeddings applied.
    """
    xq_ = torch.view_as_complex(xq.float().reshape(*xq.shape[:-1], -1, 2))
    xk_ = torch.view_as_complex(xk.float().reshape(*xk.shape[:-1], -1, 2))
    freqs_cis = reshape_for_broadcast(freqs_cis, xq_)
    xq_out = torch.view_as_real(xq_ * freqs_cis).flatten(3)
    xk_out = torch.view_as_real(xk_ * freqs_cis).flatten(3)
    return xq_out.type_as(xq), xk_out.type_as(xk)

def repeat_kv(x: torch.Tensor, n_rep: int) -> torch.Tensor:
    """
    Repeat the key and value tensors for multi-query attention.
    
    This function is used in the implementation of grouped-query attention, where
    multiple query heads share the same key and value heads. It repeats the key and value
    tensors to match the number of query heads.

    Args:
        x (torch.Tensor): The input tensor to repeat (key or value).
        n_rep (int): The number of times to repeat the input.

    Returns:
        torch.Tensor: The repeated tensor.

    Reference: https://arxiv.org/abs/2305.13245
    """
    bs, slen, n_kv_heads, head_dim = x.shape
    if n_rep == 1:
        return x
    return (
        x[:, :, :, None, :]
        .expand(bs, slen, n_kv_heads, n_rep, head_dim)
        .reshape(bs, slen, n_kv_heads * n_rep, head_dim)
    )

class Attention(nn.Module):
    """
    Multi-head attention mechanism with support for grouped-query attention.
    
    This class implements the core attention mechanism of the transformer architecture.
    It supports grouped-query attention, where multiple query heads can share the same
    key and value heads, reducing computational complexity.

    Reference for grouped-query attention: https://arxiv.org/abs/2305.13245
    """
    def __init__(self, args: ModelArgs):
        super().__init__()
        self.n_kv_heads = args.n_heads if args.n_kv_heads is None else args.n_kv_heads
        model_parallel_size = fs_init.get_model_parallel_world_size()
        self.n_local_heads = args.n_heads // model_parallel_size
        self.n_local_kv_heads = self.n_kv_heads // model_parallel_size
        self.n_rep = self.n_local_heads // self.n_local_kv_heads
        self.head_dim = args.dim // args.n_heads

        self.wq = ColumnParallelLinear(
            args.dim,
            args.n_heads * self.head_dim,
            bias=False,
            gather_output=False,
            init_method=lambda x: x,
        )
        self.wk = ColumnParallelLinear(
            args.dim,
            self.n_kv_heads * self.head_dim,
            bias=False,
            gather_output=False,
            init_method=lambda x: x,
        )
        self.wv = ColumnParallelLinear(
            args.dim,
            self.n_kv_heads * self.head_dim,
            bias=False,
            gather_output=False,
            init_method=lambda x: x,
        )
        self.wo = RowParallelLinear(
            args.n_heads * self.head_dim,
            args.dim,
            bias=False,
            input_is_parallel=True,
            init_method=lambda x: x,
        )

        self.cache_k = torch.zeros(
            (
                args.max_batch_size,
                args.max_seq_len,
                self.n_local_kv_heads,
                self.head_dim,
            )
        ).cuda()
        self.cache_v = torch.zeros(
            (
                args.max_batch_size,
                args.max_seq_len,
                self.n_local_kv_heads,
                self.head_dim,
            )
        ).cuda()

    def forward(
        self,
        x: torch.Tensor,
        start_pos: int,
        freqs_cis: torch.Tensor,
        mask: Optional[torch.Tensor],
    ):
        """
        Forward pass of the attention mechanism.
        
        Args:
            x (torch.Tensor): Input tensor.
            start_pos (int): Starting position for caching.
            freqs_cis (torch.Tensor): Precomputed rotary embedding frequencies.
            mask (Optional[torch.Tensor]): Attention mask tensor.

        Returns:
            torch.Tensor: Output of the attention mechanism.
        """
        bsz, seqlen, _ = x.shape
        xq, xk, xv = self.wq(x), self.wk(x), self.wv(x)

        xq = xq.view(bsz, seqlen, self.n_local_heads, self.head_dim)
        xk = xk.view(bsz, seqlen, self.n_local_kv_heads, self.head_dim)
        xv = xv.view(bsz, seqlen, self.n_local_kv_heads, self.head_dim)

        xq, xk = apply_rotary_emb(xq, xk, freqs_cis=freqs_cis)

        self.cache_k = self.cache_k.to(xq)
        self.cache_v = self.cache_v.to(xq)

        self.cache_k[:bsz, start_pos : start_pos + seqlen] = xk
        self.cache_v[:bsz, start_pos : start_pos + seqlen] = xv

        keys = self.cache_k[:bsz, : start_pos + seqlen]
        values = self.cache_v[:bsz, : start_pos + seqlen]

        # Implement grouped-query attention
        keys = repeat_kv(keys, self.n_rep)
        values = repeat_kv(values, self.n_rep)

        xq = xq.transpose(1, 2)
        keys = keys.transpose(1, 2)
        values = values.transpose(1, 2)
        scores = torch.matmul(xq, keys.transpose(2, 3)) / math.sqrt(self.head_dim)
        if mask is not None:
            scores = scores + mask
        scores = F.softmax(scores.float(), dim=-1).type_as(xq)
        output = torch.matmul(scores, values)
        output = output.transpose(1, 2).contiguous().view(bsz, seqlen, -1)
        return self.wo(output)

class FeedForward(nn.Module):
    """
    Feed-forward neural network used in transformer blocks.
    
    This class implements the position-wise feed-forward network (FFN) component
    of the transformer architecture. It uses the SwiGLU activation function,
    which is a variant of the Gaussian Error Linear Unit (GELU).

    Reference for SwiGLU: https://arxiv.org/abs/2002.05202
    """
    def __init__(
        self,
        dim: int,
        hidden_dim: int,
        multiple_of: int,
        ffn_dim_multiplier: Optional[float],
    ):
        super().__init__()
        hidden_dim = int(2 * hidden_dim / 3)
        # Apply custom dimension multiplier if provided
        if ffn_dim_multiplier is not None:
            hidden_dim = int(ffn_dim_multiplier * hidden_dim)
        hidden_dim = multiple_of * ((hidden_dim + multiple_of - 1) // multiple_of)

        self.w1 = ColumnParallelLinear(
            dim, hidden_dim, bias=False, gather_output=False, init_method=lambda x: x
        )
        self.w2 = RowParallelLinear(
            hidden_dim, dim, bias=False, input_is_parallel=True, init_method=lambda x: x
        )
        self.w3 = ColumnParallelLinear(
            dim, hidden_dim, bias=False, gather_output=False, init_method=lambda x: x
        )

    def forward(self, x):
        # Implement SwiGLU activation function
        return self.w2(F.silu(self.w1(x)) * self.w3(x))

class TransformerBlock(nn.Module):
    """
    Single transformer block consisting of self-attention and feed-forward layers.
    
    This class implements a single block of the transformer architecture, which includes
    a multi-head self-attention mechanism followed by a position-wise feed-forward network.
    It also incorporates residual connections and layer normalization.

    Reference: https://arxiv.org/abs/1706.03762 (original Transformer paper)
    """
    def __init__(self, layer_id: int, args: ModelArgs):
        super().__init__()
        self.n_heads = args.n_heads
        self.dim = args.dim
        self.head_dim = args.dim // args.n_heads
        self.attention = Attention(args)
        self.feed_forward = FeedForward(
            dim=args.dim,
            hidden_dim=4 * args.dim,
            multiple_of=args.multiple_of,
            ffn_dim_multiplier=args.ffn_dim_multiplier,
        )
        self.layer_id = layer_id
        self.attention_norm = RMSNorm(args.dim, eps=args.norm_eps)
        self.ffn_norm = RMSNorm(args.dim, eps=args.norm_eps)

    def forward(
        self,
        x: torch.Tensor,
        start_pos: int,
        freqs_cis: torch.Tensor,
        mask: Optional[torch.Tensor],
    ):
        """
        Forward pass of the transformer block.
        
        Args:
            x (torch.Tensor): Input tensor.
            start_pos (int): Starting position for attention caching.
            freqs_cis (torch.Tensor): Precomputed rotary embedding frequencies.
            mask (Optional[torch.Tensor]): Attention mask tensor.

        Returns:
            torch.Tensor: Output of the transformer block.
        """
        # Apply attention with a residual connection
        h = x + self.attention(self.attention_norm(x), start_pos, freqs_cis, mask)
        # Apply feed-forward layer with a residual connection
        out = h + self.feed_forward(self.ffn_norm(h))
        return out

class Transformer(nn.Module):
    """
    Main transformer model class.
    
    This class implements the full transformer architecture, consisting of
    an embedding layer, multiple transformer blocks, and an output layer.
    It also handles the generation of rotary positional embeddings.

    Reference: https://arxiv.org/abs/1706.03762 (original Transformer paper)
    """
    def __init__(self, params: ModelArgs):
        super().__init__()
        self.params = params
        self.vocab_size = params.vocab_size
        self.n_layers = params.n_layers

        self.tok_embeddings = VocabParallelEmbedding(
            params.vocab_size, params.dim, init_method=lambda x: x
        )

        self.layers = torch.nn.ModuleList()
        for layer_id in range(params.n_layers):
            self.layers.append(TransformerBlock(layer_id, params))

        self.norm = RMSNorm(params.dim, eps=params.norm_eps)
        self.output = ColumnParallelLinear(
            params.dim, params.vocab_size, bias=False, init_method=lambda x: x
        )

        self.freqs_cis = precompute_freqs_cis(
            params.dim // params.n_heads,
            params.max_seq_len * 2,
            params.rope_theta,
            params.use_scaled_rope,
        )

    @torch.inference_mode()
    def forward(self, tokens: torch.Tensor, start_pos: int):
        """
        Forward pass of the transformer model.
        
        This method processes the input tokens through the entire transformer architecture,
        including embedding, multiple transformer blocks, and the final output layer.

        Args:
            tokens (torch.Tensor): Input token indices.
            start_pos (int): Starting position for attention caching.

        Returns:
            torch.Tensor: Output logits for next token prediction.
        """
        _bsz, seqlen = tokens.shape
        h = self.tok_embeddings(tokens)
        self.freqs_cis = self.freqs_cis.to(h.device)
        freqs_cis = self.freqs_cis[start_pos : start_pos + seqlen]

        # Create causal mask for self-attention
        mask = None
        if seqlen > 1:
            mask = torch.full((seqlen, seqlen), float("-inf"), device=tokens.device)
            mask = torch.triu(mask, diagonal=1)
            mask = torch.hstack(
                [torch.zeros((seqlen, start_pos), device=tokens.device), mask]
            ).type_as(h)

        # Process through transformer blocks
        for layer in self.layers:
            h = layer(h, start_pos, freqs_cis, mask)
        
        h = self.norm(h)
        output = self.output(h).float()
        return output

# Advanced usage example:
# 
# def generate_text(model, tokenizer, prompt, max_length=100):
#     model.eval()
#     input_ids = tokenizer.encode(prompt, return_tensors="pt").to(model.device)
#     
#     for _ in range(max_length):
#         with torch.no_grad():
#             outputs = model(input_ids, start_pos=0)
#         
#         next_token_logits = outputs[:, -1, :]
#         next_token = torch.argmax(next_token_logits, dim=-1).unsqueeze(-1)
#         input_ids = torch.cat([input_ids, next_token], dim=-1)
#         
#         if next_token.item() == tokenizer.eos_token_id:
#             break
#     
#     return tokenizer.decode(input_ids[0])
# 
# # Usage:
# # model = Transformer(ModelArgs(...))
# # tokenizer = YourTokenizer(...)  # You'd need to implement or import a suitable tokenizer
# # generated_text = generate_text(model, tokenizer, "Once upon a time")
# # print(generated_text)

# Note on model parallelism:
# This implementation uses FairScale's model parallel primitives (ColumnParallelLinear, 
# RowParallelLinear, VocabParallelEmbedding) to enable efficient distributed training 
# across multiple GPUs. These layers automatically handle the distribution and 
# communication of model parameters and gradients across devices.

# Performance considerations:
# 1. The use of RMSNorm instead of LayerNorm can lead to faster training and inference.
# 2. Grouped-query attention reduces the computational complexity of the attention mechanism.
# 3. The SwiGLU activation in the feed-forward layers can improve model performance.
# 4. Rotary positional embeddings allow for better handling of variable-length sequences.

# Potential improvements and extensions:
# 1. Implement more advanced decoding strategies (e.g., beam search, top-k, top-p sampling).
# 2. Add support for different attention variants (e.g., sparse attention, linear attention).
# 3. Incorporate techniques for longer context handling (e.g., memory-efficient attention).
# 4. Implement gradient checkpointing for more memory-efficient training.
# 5. Add support for different tokenization schemes and mixed precision training.

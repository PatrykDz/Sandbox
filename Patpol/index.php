<html>
<?php

$invoiceContent = [
    'InvoiceContent' => [
        'price' => 6.69,          // The unit price of the item
        'count' => 27,              // The quantity of the item
        'discount_amount' => 8.91  // The total discount amount
    ]
];

// Assuming $xml is an object with methods currency and niceFloat
$xml = new class {
    public function currency($amount) {
        return $amount; // Assuming this method formats or processes the currency value
    }

    public function niceFloat($number) {
        return $number; // Assuming this method formats or processes the float value
    }
};
?>

Equation: 
<?= 
    round(
        (
            (($xml->currency($invoiceContent['InvoiceContent']['price']) * $xml->niceFloat($invoiceContent['InvoiceContent']['count'])) 
            - ($xml->currency($invoiceContent['InvoiceContent']['discount_amount']))) / $invoiceContent['InvoiceContent']['count']
        ), 2)
?>
</html>
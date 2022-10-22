use rhai::{Engine};

fn main() {
    let engine = Engine::new();

    let result = engine.eval_file::<String>("my_script.rhai".into()).unwrap();

    println!("{}", result);
}
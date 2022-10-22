use std::net::TcpListener;
use std::net::TcpStream;
use std::io::prelude::*;

fn main() {
  let listener = TcpListener::bind("127.0.0.1:1234").unwrap();
  println!("Listening for connections...");

  listener.

  for stream in listener.incoming() {
    let s: TcpStream = stream.unwrap();

    println!("Connection extablished!");
    println!("{}", s.peer_addr().unwrap());

    handle_connection(s)
  }
}

fn handle_connection(mut stream: TcpStream) {
  let mut buffer: [u8; 1024] = [0; 1024];
  stream.read(&mut buffer).unwrap();
  println!("Request: {}",
          String::from_utf8_lossy(&buffer[..]));

  let response = "HTTP/1.1 200 OK\r\n\r\n";
  stream.write(response.as_bytes()).unwrap();
  stream.flush().unwrap();
}
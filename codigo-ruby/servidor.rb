require "ffi-rzmq"
require "time"

context = ZMQ::Context.new
socket = context.socket(ZMQ::REP)

socket.bind("tcp://*:5555")

loop do 
  mensagem = ""
  socket.recv_string(mensagem)
  puts "#{Time.now.strftime("%Y-%m-%d %H:%M:%S")} | Bem vindo #{mensagem}"
  socket.send_string("OK")
end

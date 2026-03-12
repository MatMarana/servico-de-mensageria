require "ffi-rzmq"
require "time"

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
socket.connect("tcp://servidor-ruby:5555")
loop do
  socket.send_string("Tiago")
  reply = ""
  socket.recv_string(reply)
  puts "Resposta #{reply}"
end

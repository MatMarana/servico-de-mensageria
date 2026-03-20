STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

lista_nomes = ["Ale", "Gabriel", "Giovanni", "Kawan", "Pedro", "Roberto", "Leo", "Henrique"]
lista_canais = []

context = ZMQ::Context.new
socket = context.socket(ZMQ::REP)

socket.connect("tcp://broker:5556")

loop do 
  string = ""
  socket.recv_string(string)
  mensagem = MessagePack.unpack(string)
  puts("#{mensagem["mensagem"]}")

  if lista_nomes.include?(mensagem["nome"])
    reply = {status: "erro"}.to_msgpack
    socket.send_string(reply)
  else
    reply = {status: "login"}.to_msgpack
    socket.send_string(reply)
    break
  end

end

loop do
  string = ""
  socket.recv_string(string)
  mensagem = MessagePack.unpack(string)

  puts "#{mensagem["mensagem"]}"

  if lista_canais.include?(mensagem["canal"])
    reply = {status: "erro"}.to_msgpack
    socket.send_string(reply)
    break
  else
    reply = {status: "sucesso"}.to_msgpack
    socket.send_string(reply)
    lista_canais << mensagem["canal"]
  end

end

loop do 
  string = ""
  socket.recv_string(string)
  mensagem = MessagePack.unpack(string)

  puts "#{mensagem["mensagem"]}"
  reply = {canal: "#{lista_canais}"}.to_msgpack
  socket.send_string(reply)
end

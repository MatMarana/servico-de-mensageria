STDOUT.sync = true

require "ffi-rzmq"
require "google/protobuf"
require "time"

require_relative "ruby-proto_pb.rb"

lista_logados = []

context = ZMQ::Context.new
socket = context.socket(ZMQ::DEALER)

socket.connect("tcp://broker:5556")

loop do 
  parts = []
  socket.recv_strings(parts)

  mensagem_bin = parts.last
  mensagem = MensagemProto::NomeLogin.decode(mensagem_bin)

  puts "#{Time.now.strftime("%Y-%m-%d %H:%M:%S")} | Bem vindo #{mensagem.nome}"

  mensagem = "OK"
  reply = MensagemProto::ResponseLogin.new("resposta": mensagem)
  reply_bin = MensagemProto::ResponseLogin.encode(reply)
  parts[-1] = reply_bin
  socket.send_strings(parts)
end

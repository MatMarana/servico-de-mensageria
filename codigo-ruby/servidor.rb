STDOUT.sync = true

require "ffi-rzmq"
require "google/protobuf"
require "time"

require_relative "ruby-proto_pb.rb"
require_relative "utils.rb"

lista_nomes_permitidos = ["Henrique", "Leo", "Mateus", "Tiago"]
lista_logados = []

lista_canais = []

context = ZMQ::Context.new
socket = context.socket(ZMQ::DEALER)

socket.connect("tcp://broker:5556")

loop do 

  mensagem = Utils.recebe_request(socket)
  puts "#{mensagem.mensagem}"
  
  reply = MensagemProto::EnviaMensagem.new("mensagem": "OK")
  reply_bin = MensagemProto::EnviaMensagem.encode(reply)
  socket.send_string(reply_bin)

end

STDOUT.sync = true

require "ffi-rzmq"
require "google/protobuf"
require "time"

require_relative "ruby-proto_pb.rb"

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["Sistemas Distribuidos", "Jogos", "IA", "Engenharia de Software", "Gestão de Projetos", "TCC"]

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
socket.connect("tcp://broker:5555")

nomes_login.each do |nome|
  parts = []
  request = MensagemProto::NomeLogin.new("nome": nome)
  request_bin = MensagemProto::NomeLogin.encode(request)
  socket.send_string(request_bin)
  reply_bin = ""
  socket.recv_strings(parts)
  reply = MensagemProto::ResponseLogin.decode(parts.last)
  puts "Resposta #{reply.resposta}"
end

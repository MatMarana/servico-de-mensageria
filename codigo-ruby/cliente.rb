STDOUT.sync = true

require "ffi-rzmq"
require "google/protobuf"
require "time"

require_relative "ruby-proto_pb.rb"
require_relative "utils.rb"

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["Sistemas Distribuidos", "Jogos", "IA", "Engenharia de Software", "Gestão de Projetos", "TCC"]

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
socket.connect("tcp://broker:5555")

nomes_login.each do |nome|
  parts = []
  
  request_bin = Utils.cria_request('login', nome)
  socket.send_string(request_bin)

  mensagem_retorno = Utils.recebe_request(socket)

  putus "mensagem: #{mensagem_retorno.mesagem}"
end

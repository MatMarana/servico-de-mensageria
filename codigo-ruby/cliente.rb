STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["Sistemas Distribuidos", "Jogos", "IA", "Engenharia de Software", "Gestão de Projetos", "TCC"]

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
socket.connect("tcp://broker:5555")

loop do
  nome = nomes_login.sample
  string = ""
  time = Time.now.strftime("%H:%M:%S")
  
  mensagem_formatada = "login | #{nome} | #{time}"

  mensagem = {nome: nome, mensagem: mensagem_formatada}.to_msgpack

  socket.send_string(mensagem)
  
  socket.recv_string(string)
  resposta = MessagePack.unpack(string)
  puts "#{resposta["status"]}"
  
  if resposta["status"] == "login"
    break
  end

  sleep(0.5)

end

loop do
  canal = nomes_canais.sample
  string = ""
  time = Time.now.strftime("%H:%M:%S")

  mensagem_formatada = "canais | #{canal} | #{time}"

  mensagem = {canal: canal, mensagem: mensagem_formatada}.to_msgpack

  socket.send_string(mensagem)

  socket.recv_string(string)
  resposta = MessagePack.unpack(string)
  puts "#{resposta["status"]}"

  if resposta["status"] == "erro"
    break
  end

  sleep(0.5)
end

loop do 
  string = ""
  time = Time.now.strftime("%H:%M:%S")

  mensagem_formatada = "listar |  | #{time}"

  mensagem = {mensagem: mensagem_formatada}.to_msgpack

  socket.send_string(mensagem)
  
  socket.recv_string(string)
  resposta = MessagePack.unpack(string)

  puts "#{resposta["canal"]}"

  sleep(0.5)
end

STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["IA", "TCC", "ESTRUTURA DE DADOS", "COMPLEXIDADE DE ALGORITMOS", "ARQUITETURA DE COMPUTADORES"]
canais_inscritos = []

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
socket.connect("tcp://broker:5555")

subscriber = context.socket(ZMQ::SUB)
subscriber.connect("tcp://proxy:5557")
subscriber.setsockopt(ZMQ::SUBSCRIBE, "")

loop do
  nome = nomes_login.sample
  string = ""
  time = Time.now.strftime("%H:%M:%S")
  
  mensagem_formatada = "login|#{nome}|#{time}"
  puts "#{mensagem_formatada}"

  mensagem = (mensagem_formatada).to_msgpack
  socket.send_string(mensagem)
  
  socket.recv_string(string)
  resposta = MessagePack.unpack(string)
  
  if resposta == "login"
    break
  end

  sleep(1)

end

nomes_canais.each do |canal|
  string = ""
  time = Time.now.strftime("%H:%M:%S")

  mensagem_formatada = "canais|#{canal}|#{time}"
  puts "#{mensagem_formatada}"

  mensagem = (mensagem_formatada).to_msgpack
  socket.send_string(mensagem)

  socket.recv_string(string)
  resposta = MessagePack.unpack(string)

  if resposta == "erro"
    break
  end

  sleep(1)
end
 
string = ""
time = Time.now.strftime("%H:%M:%S")

mensagem_formatada = "listar||#{time}"

puts "#{mensagem_formatada}"

mensagem = (mensagem_formatada).to_msgpack
socket.send_string(mensagem)
  
socket.recv_string(string)
resposta = MessagePack.unpack(string)

canais_inscritos = resposta.scan(/:\s*(.+)/).flatten.map(&:strip)

sleep(1)

loop do
  mensagem_teste_bin = ''
  subscriber.recv_string(mensagem_teste_bin)
  mensagem_teste = MessagePack.unpack(mensagem_teste_bin)

  puts "#{mensagem_teste}"
end

STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["IA", "TCC", "ESTRUTURA DE DADOS", "COMPLEXIDADE DE ALGORITMOS", "ARQUITETURA DE COMPUTADORES", "EOF"]

canais_cadastrados = []
canais_inscritos = []

relogio_cliente = 0
contador = 0

context = ZMQ::Context.new

socket = context.socket(ZMQ::REQ)
subscriber = context.socket(ZMQ::SUB)

socket.connect("tcp://broker:5555")
subscriber.connect("tcp://proxy:5557")

loop do
  nome = nomes_login.sample #Pega um nome de forma aleatória
  time = Time.now.strftime("%H:%M:%S")

  relogio_cliente += 1 #Incrementa o relógio lógico

  mensagem_formatada = "login|#{nome}|#{time}|relogio: #{relogio_cliente}"
  puts "#{mensagem_formatada}"

  socket.send_string((mensagem_formatada).to_msgpack)
  
  sleep(1)

  string = ""
  socket.recv_string(string)
  resposta = MessagePack.unpack(string)

  partes = resposta.split("|")
  resultado = partes[0]
  relogio_servidor = partes[1]

  if relogio_servidor.to_i > relogio_cliente
    relogio_cliente = relogio_servidor.to_i
  end

  if resultado == "login"
    break
  end

  sleep(1)

end

nomes_canais.each do |canal|
  time = Time.now.strftime("%H:%M:%S")
  relogio_cliente += 1

  mensagem_formatada = "canais|#{canal}|#{time}|relogio: #{relogio_cliente}"
  puts "#{mensagem_formatada}"

  mensagem = (mensagem_formatada).to_msgpack
  socket.send_string(mensagem)

  sleep(1)

  string = ""
  socket.recv_string(string)
  resposta = MessagePack.unpack(string)

  partes = resposta.split("|")
  resultado = partes[0]
  relogio_servidor = partes[1]

  if relogio_servidor.to_i > relogio_cliente
    relogio_cliente = relogio_servidor.to_i
  end

  if resultado  == "erro"
    break
  end

  sleep(1)
end

time = Time.now.strftime("%H:%M:%S")
relogio_cliente += 1

mensagem_formatada = "listar||#{time}|relogio: #{relogio_cliente}"
puts "#{mensagem_formatada}"

mensagem = (mensagem_formatada).to_msgpack
socket.send_string(mensagem)

sleep(1)

string = ""
socket.recv_string(string)
resposta = MessagePack.unpack(string)

canais_cadastrados = resposta.scan(/:\s*(.+)/).flatten.map(&:strip)

sleep(1)

3.times do
  canal = canais_cadastrados.sample
  canais_inscritos << canal
  subscriber.setsockopt(ZMQ::SUBSCRIBE, canal)
end

loop do
  canal = canais_inscritos.sample
  time = Time.now.strftime("%H:%M:%S")
  relogio_cliente += 1

  mensagem_cliente = "canal|#{canal}-Mensagem Numero #{contador}|#{time}|relogio: #{relogio_cliente}"

  mensagem_cliente_bin = (mensagem_cliente).to_msgpack
  socket.send_string(mensagem_cliente_bin)
  sleep(1)

  puts "#{mensagem_cliente}"

  resposta = ''

  socket.recv_string(resposta)

  partes = resposta.split("|")
  resultado = partes[0]
  relogio_servidor = partes[1]

  if relogio_servidor.to_i > relogio_cliente
    relogio_cliente = relogio_servidor.to_i
  end

  topico = ""
  subscriber.recv_string(topico)

  sleep(1)

  mensagem_publicada = ""
  subscriber.recv_string(mensagem_publicada)
  puts "RECEBENDO: #{topico} | MSG: #{mensagem_publicada}"

  contador += 1

  sleep(1)

end

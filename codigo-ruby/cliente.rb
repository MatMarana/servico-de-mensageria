STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

require_relative "utils"

def receive_format_message(socket)
  resposta = Utils.receive_message(socket)
  divisao = resposta.split("|")
  resultado = divisao[0]
  relogio = divisao[1].split(": ")
  relogio_servidor = relogio[1]

  return resultado, relogio_servidor
end

nomes_login = ["Ale", "Gabriel", "Giovanni", "Henrique", "Kawan", "Leo", "Mateus", "Pedro", "Roberto", "Tiago"]
nomes_canais = ["IA", "TCC", "ESTRUTURA DE DADOS", "COMPLEXIDADE DE ALGORITMOS", "ARQUITETURA DE COMPUTADORES", "EOF"]

canais_cadastrados = []
canais_inscritos = []

relogio_cliente = 0
contador = 0

context = ZMQ::Context.new

socket, subscriber = Utils.create_context_ZMQ(context, false)

loop do
  nome = nomes_login.sample #Pega um nome de forma aleatória
  time = Time.now.strftime("%H:%M:%S")

  relogio_cliente += 1 #Incrementa o relógio lógico

  mensagem_formatada = "login|#{nome}|#{time}|relogio: #{relogio_cliente}"
  puts "#{mensagem_formatada}"

  Utils.send_message(socket, mensagem_formatada)
  
  sleep(1)

  resultado, relogio_servidor = receive_format_message(socket)

  relogio_cliente = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  if resultado == "login"
    break
  end

  sleep(1)

end

nomes_canais.each do |canal|
  time = Time.now.strftime("%H:%M:%S")
  
  relogio_cliente += 1 #Incrementa relógio lógico

  mensagem_formatada = "canais|#{canal}|#{time}|relogio: #{relogio_cliente}"
  puts "#{mensagem_formatada}"

  Utils.send_message(socket, mensagem_formatada)

  sleep(1)

  resultado, relogio_servidor = receive_format_message(socket)

  relogio_cliente = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  if resultado  == "erro"
    break
  end

  sleep(1)
end

time = Time.now.strftime("%H:%M:%S")

relogio_cliente += 1

mensagem_formatada = "listar||#{time}|relogio: #{relogio_cliente}"
puts "#{mensagem_formatada}"

Utils.send_message(socket, mensagem_formatada)

sleep(1)

resposta = Utils.receive_message(socket)

canais_cadastrados = resposta.scan(/:\s*(.+)/).flatten.map(&:strip)

sleep(1)

3.times do
  canal = canais_cadastrados.sample
  canais_inscritos << canal
  subscriber.setsockopt(ZMQ::SUBSCRIBE, canal)
end

loop do
  topico = ""
  mensagem_publicada = ""
  canal = canais_inscritos.sample
  time = Time.now.strftime("%H:%M:%S")

  relogio_cliente += 1

  mensagem_cliente = "canal|#{canal}-mensagem numero #{contador}|#{time}|relogio: #{relogio_cliente}"

  Utils.send_message(socket, mensagem_cliente)

  puts "#{mensagem_cliente}"
  
  sleep(1)

  resultado, relogio_servidor = receive_format_message(socket)

  relogio_cliente = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  subscriber.recv_string(topico)

  sleep(1)

  subscriber.recv_string(mensagem_publicada)
  puts "RECEBENDO: #{topico} | MSG: #{mensagem_publicada}"

  contador += 1

  sleep(1)

end

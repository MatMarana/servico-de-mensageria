STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

require_relative "utils"

def monta_resposta_canais(lista_canais)
  reply = ""
  contador = 0
  lista_canais.each  do |canal|
    reply += "canal #{contador}: #{canal} \n"
    contador += 1
  end
  return reply
end

lista_nomes = ["Ale", "Gabriel", "Giovanni", "Kawan", "Pedro", "Roberto", "Leo", "Henrique"]

lista_canais = []

relogio_servidor = 0
contador_mensagens = 0

nome_servidor = "servidor-ruby"

context = ZMQ::Context.new

socket, publisher, reference = Utils.create_context_ZMQ(context, true)

mensagem = "#{nome_servidor}"
Utils.send_message(reference, mensagem)
rank = Utils.receive_message(reference)
puts "#{rank}"

loop do
  mensagem = Utils.receive_message(socket)

  contador_mensagens += 1

  partes = mensagem.split("|")
  operacao = partes[0]
  informacao = partes[1]
  tempo = partes[2]
  relogio_divido = partes[3].split(": ")
  relogio_cliente = relogio_divido[1] 

  relogio_servidor = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  relogio_servidor += 1

  case operacao
    when "login"
      if lista_nomes.include?(informacao)
        reply = "erro|relogio: #{relogio_servidor}"
      else
        reply = "login|relogio: #{relogio_servidor}"
      end
    when "canais"
      if informacao == "EOF"
        reply = "erro|relogio: #{relogio_servidor}"
      else
        reply = "sucesso|relogio: #{relogio_servidor}"
        lista_canais << informacao
      end
    when "listar"
      reply = monta_resposta_canais(lista_canais)
    when "canal"
      conteudo = informacao.split("-")
      canal = conteudo[0]
      mensagem = conteudo[1]
      if conteudo
        reply = "ok|relogio: #{relogio_servidor}"
      else
        reply = "erro|relogio: #{relogio_servidor}"
      end
      publisher.send_string(canal, ZMQ::SNDMORE)
      sleep(1)
      publisher.send_string(mensagem)
      puts "PUBLICANDO: #{canal} | MSG: #{mensagem}"
  end
  sleep(1)
  Utils.send_message(socket, reply)
  puts "#{reply}"
  sleep(1)

  Utils.send_message(reference, "listar")
  lista_servidores = Utils.receive_message(reference)
  puts "#{lista_servidores}"

  if contador_mensagens == 10
    mensagem = "#{nome_servidor}"
    Utils.send_message(reference, mensagem)
    hora = Utils.receive_message(reference)
  end

end

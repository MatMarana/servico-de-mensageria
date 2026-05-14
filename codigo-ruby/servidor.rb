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

$coordenador = nil

context = ZMQ::Context.new

socket, publisher, subscriber, reference = Utils.create_context_ZMQ(context, true)

subscriber.setsockopt(ZMQ::SUBSCRIBE, "server")
sleep(1)

# Cadastra servidor na lista de ranks - Usado na referência
# mensagem = "registro|#{nome_servidor}|#{relogio_servidor}"
# Utils.send_message(reference, mensagem)
# resposta = Utils.receive_message(reference)
# puts "#{resposta}"

# Thread.new do
#   loop do
#     topico = ""
#     mensagem = ""

#     subscriber.recv_string(topico)
#     sleep(1)
#     subscriber.recv_string(mensagem)
#     $coordenador = mensagem

#     puts "RECEBENDO #{topico} | MSG: #{mensagem}"  
#     puts "Novo coordenador: #{mensagem}"
#   end
# end

loop do

  mensagem = Utils.receive_message(socket)

  contador_mensagens += 1

  partes = mensagem.split("|")
  operacao = partes[0]
  informacao = partes[1]
  tempo = partes[2]
  relogio_divido = partes[3].split(":")
  relogio_cliente = relogio_divido[1] 

  relogio_servidor = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  relogio_servidor += 1

  case operacao
    when "login"
      if lista_nomes.include?(informacao)
        reply = "erro|relogio:#{relogio_servidor}"
      else
        reply = "login|relogio:#{relogio_servidor}"
      end
    when "canais"
      if informacao == "EOF"
        reply = "erro|relogio:#{relogio_servidor}"
      else
        reply = "sucesso|relogio:#{relogio_servidor}"
        lista_canais << informacao
      end
    when "listar"
      reply = monta_resposta_canais(lista_canais)
    when "canal"
      conteudo = informacao.split("-")
      canal = conteudo[0]
      mensagem = conteudo[1]
      if conteudo
        reply = "ok|relogio:#{relogio_servidor}"
      else
        reply = "erro|relogio:#{relogio_servidor}"
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

  # if contador_mensagens % 15 == 0
  #   mensagem = "heartbeat|#{nome_servidor}|#{relogio_servidor}"
  #   puts"#{mensagem}"
  #   Utils.send_message(reference, mensagem)

  #   resposta = Utils.receive_message(reference)

  #   mensagem = "relogio|#{nome_servidor}|#{relogio_servidor}"
  #   puts "#{mensagem}"
  #   Utils.send_message(reference, mensagem)

  #   resposta = Utils.receive_message(reference)

  #   if resposta == "SERVIDOR_REMOVIDO"
  #     mensagem = "eleicao||"
  #     puts "#{mensagem}"
  #     Utils.send_message(reference,mensagem)

  #     resposta = Utils.receive_message(reference)
  #     partes = resposta.split("|")
  #     $coordenador = partes[1]
  #     puts "Novo coordenador #{$coordenador}"

  #     if $coordenador == nome_servidor
  #       publisher.send_string("server", ZMQ::SNDMORE)
  #       sleep(1)
  #       publisher.send_string("servidor-ruby")
  #       sleep(1)

  #       puts "PUBLICANDO: server | MSG: servidor-ruby"
  #     end

  #   else
  #     partes = resposta.split("|")
  #     nova_hora = partes[1].to_i
  #     relogio_servidor = nova_hora
  #     puts "Relógio sincronizado #{relogio_servidor}"
  #   end
  # end

  # Utils.send_message(reference, "listar||")
  # lista_servidores = Utils.receive_message(reference)
  # puts "#{lista_servidores}"
  
end
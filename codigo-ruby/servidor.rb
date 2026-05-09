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

context = ZMQ::Context.new

socket, publisher = Utils.create_context_ZMQ(context, true)

reference = context.socket(ZMQ::REQ)
reference.connect("tcp://refrencia:5560")

loop do
  mensagem = Utils.receive_message(socket)

  partes = mensagem.split("|")
  operacao = partes[0]
  informacao = partes[1]
  tempo = partes[2]
  relogio_cliente = partes[3]

  relogio_servidor = Utils.get_bigger_clock(relogio_cliente, relogio_servidor)

  relogio_servidor += 1

  case operacao
    when "login"
      if lista_nomes.include?(informacao)
        reply = "erro|#{relogio_servidor}"
        Utils.send_message(socket, reply)
      else
        reply = "login|#{relogio_servidor}"
        Utils.send_message(socket, reply)
      end
    when "canais"
      if informacao == "EOF"
        reply = "erro|#{relogio_servidor}"
        Utils.send_message(socket, reply)
      else
        reply = "sucesso|#{relogio_servidor}"
        Utils.send_message(socket, reply)
        lista_canais << informacao
      end
    when "listar"
      reply = monta_resposta_canais(lista_canais)
      Utils.send_message(socket, reply)
      sleep(1)
      puts "#{reply}"
    when "canal"
      conteudo = informacao.split("-")
      canal = conteudo[0]
      mensagem = conteudo[1]
      if conteudo
        reply = "ok|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      else
        reply = "erro|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      end
      publisher.send_string(canal, ZMQ::SNDMORE)
      sleep(1)
      publisher.send_string(mensagem)
      puts "PUBLICANDO: #{canal} | MSG: #{mensagem}"
  end
  sleep(1)
  puts "#{reply}"
  sleep(1)
end

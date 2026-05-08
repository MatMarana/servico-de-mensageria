STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

lista_nomes = ["Ale", "Gabriel", "Giovanni", "Kawan", "Pedro", "Roberto", "Leo", "Henrique"]
lista_canais = []
relogio_servidor = 0

context = ZMQ::Context.new
socket = context.socket(ZMQ::REP)
publisher = context.socket(ZMQ::PUB)

socket.connect("tcp://broker:5556")
publisher.connect("tcp://proxy:5558")

loop do
  string = ""
  socket.recv_string(string)
  mensagem = MessagePack.unpack(string)

  partes = mensagem.split("|")
  operacao = partes[0]
  informacao = partes[1]
  tempo = partes[2]
  relogio_cliente = partes[3]

  if relogio_cliente.to_i > relogio_servidor
    relogio_servidor = relogio_cliente.to_i
  end  

  relogio_servidor += 1

  case operacao
    when "login"
      if lista_nomes.include?(informacao)
        reply = "erro|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      else
        reply = "login|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      end
    when "canais"
      if informacao == "EOF"
        reply = "erro|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      else
        reply = "sucesso|#{relogio_servidor}"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
        lista_canais << informacao
      end
    when "listar"
      reply = ""
      contador = 0
      lista_canais.each  do |canal|
        reply += "canal #{contador}: #{canal} \n"
        contador += 1
      end
      reply_bin = (reply).to_msgpack
      socket.send_string(reply_bin)
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

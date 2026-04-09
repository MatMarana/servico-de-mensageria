STDOUT.sync = true

require "ffi-rzmq"
require "msgpack"
require "time"

lista_nomes = ["Ale", "Gabriel", "Giovanni", "Kawan", "Pedro", "Roberto", "Leo", "Henrique"]
lista_canais = []

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

  case operacao
    when "login"
      if lista_nomes.include?(informacao)
        reply = "erro"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      else
        reply = "login"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      end
    when "canais"
      if lista_canais.include?(informacao)
        reply = "erro"
        reply_bin = (reply).to_msgpack
        socket.send_string(reply_bin)
      else
        reply = "sucesso"
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
      break
  end
  sleep(1)
  puts "#{reply}"
  
end

  loop do
    mensagem_teste = "Mensagem publicada pelo servidor"
    mensagem_teste_bin = (mensagem_teste).to_msgpack
    publisher.send_string(mensagem_teste_bin)
    sleep 1
  end


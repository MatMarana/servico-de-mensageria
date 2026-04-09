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
  mensagem_canal_bin = ''
  socket.recv_string(mensagem_canal_bin)

  mensagem_canal = MessagePack.unpack(mensagem_canal_bin)

  socket.send_string("OK")

  divide_mensagem = mensagem_canal.split("|")
  canal = divide_mensagem[0]
  mensagem = divide_mensagem[1]

  publisher.send_string(canal, ZMQ::SNDMORE)

  mensagem_publicada_bin = (mensagem).to_msgpack
  publisher.send_string(mensagem_publicada_bin)
  sleep(1)
    
  puts "#{canal}"
  puts "#{mensagem}"

end


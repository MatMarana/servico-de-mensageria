require "msgpack"

module Utils
    def self.create_context_ZMQ(context, servidor)
        if servidor
            socket = context.socket(ZMQ::REP)
            publisher = context.socket(ZMQ::PUB)
            reference = context.socket(ZMQ::REQ)

            socket.connect("tcp://broker:5556")
            publisher.connect("tcp://proxy:5558")
            reference.connect("tcp://refrencia:5559")
            
            return socket, publisher, reference
        end

        socket = context.socket(ZMQ::REQ)
        subscriber = context.socket(ZMQ::SUB)

        socket.connect("tcp://broker:5555")
        subscriber.connect("tcp://proxy:5557")

        return socket, subscriber
    end

    def self.send_message(socket, message)
        mensagem_bin = (message).to_msgpack
        socket.send_string(mensagem_bin)
    end

    def self.receive_message(socket)
        string = ''
        socket.recv_string(string)
        resposta = MessagePack.unpack(string)
        return resposta
    end

    def self.get_bigger_clock(relogio_cliente, relogio_servidor)
        maior = [relogio_cliente.to_i, relogio_servidor.to_i].max
        return maior
    end
end
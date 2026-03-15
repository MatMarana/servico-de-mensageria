import zmq
import msgpack
import time
from datetime import datetime

ARQUIVO_CADASTRO = "usuarioCadastrado.txt"


def carregar_usuarios():
    with open(ARQUIVO_CADASTRO, "r") as f:
        conteudo = f.read().strip()
        return conteudo.split(",")


context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

print("---------------------------------")
print("Servidor Iniciado", flush=True)

usuarios = carregar_usuarios()

while True:

    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)

    

    lista_msg = mensagem.split("|")
    operacao = lista_msg[0]
    conteudo = lista_msg[1]
    timestamp = lista_msg[2]

    if operacao == "login":

        if conteudo in usuarios:
            resposta = "erro"

        else:
            resposta = "login"

    else:
        resposta = "erro"

    resposta = resposta.strip().lower()
    print(resposta, flush=True)
    time.sleep(0.5)
    socket.send(msgpack.packb(resposta))
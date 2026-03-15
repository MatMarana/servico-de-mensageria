import zmq
import msgpack
from datetime import datetime

ARQUIVO = "usuarioLogin.txt"


def carregar_usuarios():
    with open(ARQUIVO, "r") as f:
        conteudo = f.read().strip()
        return conteudo.split(",")



context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

print("Servidor rodando...")
usuarios = carregar_usuarios() 
while True:

    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin).decode()

    print("Mensagem recebida:", mensagem)

    lista_msg = mensagem.split("|")
    operacao = lista_msg[0]
    conteudo = lista_msg[1]
    timestamp = lista_msg[2]
    

    # "switch"
    if operacao == "LOGIN":

        if conteudo in usuarios:

            resposta = f"ERRO|Usuario ja existe|{datetime.now()}"

        else:

            resposta = f"OK|Login realizado|{datetime.now()}"

    else:

        resposta = f"ERRO|Operacao desconhecida|{datetime.now()}"

    socket.send(msgpack.packb(resposta))
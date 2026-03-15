import zmq
import msgpack
import time
from datetime import datetime

ARQUIVO_LOGIN = "usuarioLogin.txt"


def carregar_nomes():
    with open(ARQUIVO_LOGIN, "r") as f:
        conteudo = f.read().strip()
        return conteudo.split(",")


context = zmq.Context()
socket = context.socket(zmq.REQ)

# IMPORTANTE para docker
socket.connect("tcp://servidor-python:5555")

nomes = carregar_nomes()

logado = False

for usuario in nomes:

    mensagem = f"LOGIN|{usuario}|{datetime.now()}"
    mensagem = mensagem.strip().lower()
    print(mensagem, flush=True)
    time.sleep(0.5)
    socket.send(msgpack.packb(mensagem))


    resposta_bin = socket.recv()
    resposta = msgpack.unpackb(resposta_bin, raw=False)

    

    if resposta == "login":
        logado = True
        break

    elif resposta == "erro":
        ...

if not logado:
    print("Nenhum usuario disponivel para login.", flush=True)
import zmq
import msgpack
import time
from datetime import datetime
from random import randint

ARQUIVO_LOGIN = "usuarioLogin.txt"
ARQUIVO_CANAIS = "canais.txt"

def carregar_nomes():
    with open(ARQUIVO_LOGIN, "r") as f:
        conteudo = f.read().strip()
        return conteudo.split(",")
    
def carregar_canais():
    with open(ARQUIVO_CANAIS, "r") as f:
        conteudo = f.read().strip()
        conteudo = conteudo.split(",")
        duplicado = randint(0, len(conteudo) - 1)
        conteudo.append(conteudo[duplicado])
        return conteudo 


context = zmq.Context()
socket = context.socket(zmq.REQ)

# IMPORTANTE para docker
socket.connect("tcp://broker:5555")

#Carregando os arquivos txts:
nomes = carregar_nomes()
canais = carregar_canais()

logado = False
canal = False
listar = True

if not logado:
    for usuario in nomes:
        mensagem = f"login|{usuario}|{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(0.5)
        socket.send(msgpack.packb([mensagem]))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        if resposta == "login":
            logado = True
            break
        elif resposta == "erro":
            ...
            

if logado and not canal:
    for canal in canais:
        mensagem = f"canais|{canal}|{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(0.5)
        socket.send(msgpack.packb([mensagem]))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        if resposta == "erro":
            canal = True
            break
        elif resposta == "sucesso":
            ...

if canal and listar:
    while(listar):
        mensagem = f"listar||{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True) 
        time.sleep(0.5)   
        socket.send(msgpack.packb([mensagem]))
        resposta_bin = socket.recv()


import zmq
import msgpack
import time
from datetime import datetime
from random import randint

#Variaveis:
ARQUIVO_LOGIN = "usuarioLogin.txt"
ARQUIVO_CANAIS = "canais.txt"

#Funções:
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
    
def extrair_canais(texto:str):
    linhas = texto.strip().split("\n")
    canais = []

    for linha in linhas:
        # separa pelo ":" e pega a parte do nome
        partes = linha.split(":", 1)
        if len(partes) > 1:
            canal = partes[1].strip()
            canais.append(canal)    

#============================================


context = zmq.Context()
socket = context.socket(zmq.REQ)
sub = context.socket(zmq.SUB)


# IMPORTANTE para docker
socket.connect("tcp://broker:5555")

#Carregando os arquivos txts:
nomes = carregar_nomes()
canais = carregar_canais()

logado = False
canal_bool = False
listar = True
subscriber = False  


if not logado:
    for usuario in nomes:
        mensagem = f"login|{usuario}|{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        if resposta == "login":
            logado = True
            break
        elif resposta == "erro":
            ...


if logado and not canal_bool:
    for canal in canais:
        mensagem = f"canais|{canal}|{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        if resposta == "erro":
            canal_bool = True
            break
        elif resposta == "sucesso":
            ...

if canal_bool and listar:
    while(listar):
        mensagem = f"listar||{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        listar = False


if not listar and not subscriber:
    canais_extraidos = extrair_canais(resposta)
    lista_canais = canais_extraidos.split(",")

    # resposta_bin = socket.recv()
    # resposta = msgpack.unpackb(resposta_bin, raw=False)






    


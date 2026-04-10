import zmq
import msgpack
import time
import random
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
    return ",".join(canais)

#============================================


context = zmq.Context()
socket = context.socket(zmq.REQ)
sub = context.socket(zmq.SUB)


# IMPORTANTE para docker
socket.connect("tcp://broker:5555")
sub.connect("tcp://proxy:5555")

#Carregando os arquivos txts:
nomes = carregar_nomes()
canais = carregar_canais()

logado = False
canal_bool = False
listar = True
subscriber = True  


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


if not listar and subscriber:
    canais_extraidos = extrair_canais(resposta)
    lista_canais = canais_extraidos.split(",")
    canais_aleatorios = random.sample(lista_canais,3)
    sub.subscribe(canais_aleatorios[0])
    sub.subscribe(canais_aleatorios[1])
    sub.subscribe(canais_aleatorios[2])
    indice = random.randint(0, 2)
    i = 0
    while(subscriber):
        i +=1
        mensagem = f"canal|{canais_aleatorios[indice]}-teste{i}|{datetime.now()}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        print(resposta)
    






    


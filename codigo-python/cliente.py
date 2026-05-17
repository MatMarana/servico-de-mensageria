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

# Adicionando o relógio lógico
contador_logico = 0

# Função para incrementar o relógio lógico
# def incrementar_relogio():
#     # global contador_logico
#     contador_logico += 1
#     return contador_logico

# Função para atualizar o relógio lógico ao receber uma mensagem
def atualizar_relogio(recebido):
    global contador_logico
    contador_logico = max(contador_logico, recebido)
    return contador_logico

# Função para extrair o relógio da resposta do servidor
def extrair_relogio(resposta):
    try:
        partes = resposta.split("|")
        relogio = int(partes[-1].split(":")[-1])
        return relogio
    except:
        return 0
    
def extrair_resposta(resposta):
    try:
        partes = resposta.split("|")
        resposta_servidor = partes[0]
        return resposta_servidor
    except:
        return 0
#============================================


context = zmq.Context()
socket = context.socket(zmq.REQ)
sub = context.socket(zmq.SUB)


# IMPORTANTE para docker
socket.connect("tcp://broker:5555")
sub.connect("tcp://proxy:5557")

#Carregando os arquivos txts:
nomes = carregar_nomes()
canais = carregar_canais()

logado = False
canal_bool = False
listar = True
subscriber = True  


if not logado:
    for usuario in nomes:
        contador_logico += 1
        mensagem = f"login|{usuario}|{datetime.now()}|relogio:{contador_logico}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        relogio_recebido = extrair_relogio(resposta)
        resposta_servidor = extrair_resposta(resposta)
        atualizar_relogio(relogio_recebido)
        
        if resposta_servidor == "login":
            logado = True
            break
        elif resposta_servidor == "erro":
            ...


if logado and not canal_bool:
    for canal in canais:
        contador_logico += 1
        mensagem = f"canais|{canal}|{datetime.now()}|relogio:{contador_logico}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        relogio_recebido = extrair_relogio(resposta)
        resposta_servidor = extrair_resposta(resposta)
        atualizar_relogio(relogio_recebido)
        if resposta_servidor == "erro":
            canal_bool = True
            break
        elif resposta_servidor == "sucesso":
            ...

if canal_bool and listar:
    while(listar):
        contador_logico += 1
        mensagem = f"listar||{datetime.now()}|relogio:{contador_logico}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        relogio_recebido = extrair_relogio(resposta)
        resposta_servidor = extrair_resposta(resposta)
        atualizar_relogio(relogio_recebido)
        listar = False


if not listar and subscriber:
    canais_extraidos = extrair_canais(resposta)
    lista_canais = canais_extraidos.split(",")
    canais_aleatorios = random.sample(lista_canais,3)
    for canal in canais_aleatorios:
        sub.setsockopt_string(zmq.SUBSCRIBE, canal)
    i = 0
    while(subscriber):
        indice = random.randint(0, 2)
        i += 1
        contador_logico += 1
        mensagem = f"canal|{canais_aleatorios[indice]}-teste{i}|{datetime.now()}|relogio:{contador_logico}"
        mensagem = mensagem.strip().lower()
        print(f"{mensagem}", flush=True)
        time.sleep(1)
        socket.send(msgpack.packb(mensagem))
        resposta_bin = socket.recv()
        resposta = msgpack.unpackb(resposta_bin, raw=False)
        relogio_recebido = extrair_relogio(resposta)
        resposta_servidor = extrair_resposta(resposta)
        atualizar_relogio(relogio_recebido)
        topico = sub.recv_string()
        time.sleep(1)
        mensagem_publicada = sub.recv_string()
        print(f"RECEBENDO: {topico} | MSG:{mensagem_publicada}")
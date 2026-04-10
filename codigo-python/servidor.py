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
pub = context.socket(zmq.PUB)

socket.connect("tcp://broker:5556")
pub.connect("tcp://proxy:5558")

print("---------------------------------")
print("Servidor Iniciado", flush=True)

usuarios = carregar_usuarios()
canais_recebidos = []

def adiciona_canal(canais:list, canal: str):
    canais.append(canal)
    saida = ""
    for i, c in enumerate(canais):
        saida += f"Canal {i}: {c}\n"
    return saida


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
    elif operacao == "canais":
        if conteudo != "eof":
            saida_operacao = adiciona_canal(canais_recebidos,conteudo)
            resposta = "sucesso"
        else:
            resposta = "erro"
    elif operacao == "listar":
        resposta = saida_operacao
    elif operacao == "canal":
            lista_conteudo= conteudo.split("-")
            canal = lista_conteudo[0]
            mensagem_conteudo = lista_conteudo[1]
            pub.send_string(canal, flags=zmq.SNDMORE)
            pub.send_string(mensagem_conteudo)
            resposta = "ok" 
    else:
        resposta = "erro inesperado"

    resposta = resposta.strip().lower()
    print(f"{resposta}", flush=True)
    time.sleep(1)
    socket.send(msgpack.packb(resposta))

import zmq
import msgpack
from datetime import datetime

# Lista de servidores e ranks
servidores = {}
rank_counter = 1

# Função para registrar um servidor e atribuir um rank
def registrar_servidor(nome):
    global rank_counter
    if nome not in servidores:
        servidores[nome] = {
            "rank": rank_counter,
            "last_heartbeat": datetime.now()
        }
        rank_counter += 1
    return servidores[nome]["rank"]

# Função para retornar a lista de servidores
def listar_servidores():
    return [
        {"nome": nome, "rank": dados["rank"]}
        for nome, dados in servidores.items()
    ]

# Função para atualizar o heartbeat de um servidor
def atualizar_heartbeat(nome):
    if nome in servidores:
        servidores[nome]["last_heartbeat"] = datetime.now()
        return "OK"
    return "ERRO"

# Função para remover servidores inativos
def remover_inativos():
    agora = datetime.now()
    inativos = [
        nome for nome, dados in servidores.items()
        if (agora - dados["last_heartbeat"]).seconds > 30
    ]
    for nome in inativos:
        del servidores[nome]

# Configuração do ZeroMQ
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5560")

print("Serviço de referência iniciado", flush=True)

while True:
    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)
    operacao = mensagem["operacao"]
    conteudo = mensagem.get("conteudo", "")

    if operacao == "registrar":
        rank = registrar_servidor(conteudo)
        resposta = {"rank": rank}
    elif operacao == "listar":
        resposta = listar_servidores()
    elif operacao == "heartbeat":
        resposta = {"status": atualizar_heartbeat(conteudo)}
    else:
        resposta = {"erro": "Operação desconhecida"}

    remover_inativos()
    socket.send(msgpack.packb(resposta))
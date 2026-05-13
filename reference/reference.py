import zmq
import msgpack
from datetime import datetime

nomes_servidores = ["servidor-python", "servidor-csharp", "servidor-ruby"]
servidores = {}
rank_counter = 1

# Função para registrar um servidor e atribuir um rank
def registrar_servidor(nome):
    global rank_counter
    servidores[nome] = {
        "rank": rank_counter,
        "hora": 0
    }
    rank_counter += 1
    return servidores[nome]["rank"]

# Função para retornar a lista de servidores
def listar_servidores():
    resposta = ""
    for nome, dados in servidores.items():
        resposta += f"Nome: {nome}|Rank: {dados["rank"]}"
    
    return resposta

# Função para atualizar o heartbeat de um servidor
def atualizar_heartbeat(nome):
    servidores[nome]["hora"] = 10
    return "OK"

# Função para remover servidores inativos
def remover_inativos():
    agora = datetime.now()
    inativos = [
        nome for nome, dados in servidores.items()
        if (agora - dados["last_heartbeat"]).seconds > 30
    ]
    for nome in inativos:
        del servidores[nome]

def atualizar_hora():
    for nome, dados in servidores.items():
        if dados["rank"] == 1:
            return dados["hora"]

    return "Rank 1 não encontrado"

# Configuração do ZeroMQ
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5559")

print("Serviço de referência iniciado", flush=True)

while True:
    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)

    if mensagem == "listar":
        resposta = listar_servidores()
    elif mensagem not in servidores:
        if mensagem in nomes_servidores:
            resposta = registrar_servidor(mensagem)
    elif mensagem in servidores:
       resposta = atualizar_heartbeat(mensagem)    


    #remover_inativos()
    socket.send(msgpack.packb(resposta))
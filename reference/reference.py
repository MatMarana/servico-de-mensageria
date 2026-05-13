import zmq
import msgpack
from datetime import datetime

nomes_servidores = ["servidor-python", "servidor-csharp", "servidor-ruby"]
servidores = {}

def registrar_servidor(nome, hora):
    if nome not in servidores:
        servidores[nome] = {
            "rank": len(servidores) + 1,
            "hora": hora,
            "faltas": 0
        }

    return servidores[nome]["rank"]

# Função para retornar a lista de servidores
def listar_servidores():
    resposta = ""
    for nome, dados in servidores.items():
        resposta += f"servidor: {nome}|rank: {dados['rank']}\n"
    
    return resposta

def atualizar_heartbeat(nome, hora):
    if nome not in servidores:
        rank = registrar_servidor(nome, hora)
        return "SERVIDOR_REMOVIDO"

    servidores[nome]["hora"] = hora
    servidores[nome]["faltas"] = 0

    return atualizar_hora()

def atualizar_hora():
    if not servidores:
        return 0

    lider = min(
        servidores.items(),
        key=lambda item: item[1]["rank"]
    )

    return lider[1]["hora"]

def atualizar_faltas():
    inativos = []
    for nome, dados in servidores.items():
        dados["faltas"] += 1
        if dados["faltas"] > 9:
            inativos.append(nome)

    for nome in inativos:
        print(f"Removendo servidor: {nome}")
        del servidores[nome]

    reorganizar_ranks()

def reorganizar_ranks():
    servidores_ordenados = sorted(
        servidores.items(),
        key=lambda item: item[1]["rank"]
    )

    novo_rank = 1

    for nome, dados in servidores_ordenados:
        dados["rank"] = novo_rank
        novo_rank += 1

# Configuração do ZeroMQ
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5559")

print("Serviço de referência iniciado", flush=True)

while True:
    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)
    partes = mensagem.split("|")
    operacao = partes[0]

    if operacao != "heartbeat":
        atualizar_faltas()

    if operacao == "listar":
        resposta = listar_servidores()
    elif operacao == "registro":
        nome = partes[1]
        hora = int(partes[2])
        resposta = registrar_servidor(nome, hora)
    elif operacao == "heartbeat":
        nome = partes[1]
        hora = int(partes[2])
        resposta = atualizar_heartbeat(nome, hora)

    socket.send(msgpack.packb(resposta))
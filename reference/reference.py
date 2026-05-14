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

    reorganizar_ranks()

    return servidores[nome]["rank"]

# Função para retornar a lista de servidores
def listar_servidores():
    resposta = ""
    for nome, dados in servidores.items():
        resposta += f"servidor:{nome}|rank:{dados['rank']}\n"
    
    return resposta

def atualizar_heartbeat(nome, hora):
    if nome not in servidores:
        registrar_servidor(nome, hora)
        return "SERVIDOR_REENTROU"

    servidores[nome]["hora"] = hora
    servidores[nome]["faltas"] = 0
    coordenador = obter_coordenador()

    return (f"coordenador|{coordenador}|hora:{obter_hora_coordenador()}")

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
        if dados["faltas"] >= 15:
            inativos.append(nome)

    for nome in inativos:
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


def obter_coordenador():
    if not servidores:
        return None

    lider = min(
        servidores.items(),
        key=lambda item: item[1]["rank"]
    )

    return lider[0]


def obter_hora_coordenador():
    coordenador = obter_coordenador()
    if coordenador is None:
        return 0

    return servidores[coordenador]["hora"]

def atualizar_relogio(nome, hora):
    if nome not in servidores:
        return "SERVIDOR_REMOVIDO"

    servidores[nome]["hora"] = hora
    hora_correta = obter_hora_coordenador()
    return f"hora|{hora_correta}"

def eleger_coordenador():
    coordenador = obter_coordenador()
    return coordenador

# Configuração do ZeroMQ
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5559")

while True:
    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)
    partes = mensagem.split("|")
    operacao = partes[0]

    if operacao == "listar":
        atualizar_faltas()
        resposta = listar_servidores()
    elif operacao == "registro":
        nome = partes[1]
        hora = int(partes[2])
        rank = registrar_servidor(nome, hora)
        coordenador = obter_coordenador()
        resposta = (f"rank:{rank}|coordenador:{coordenador}")
    elif operacao == "heartbeat":
        nome = partes[1]
        hora = int(partes[2])
        resposta = atualizar_heartbeat(nome, hora)
    elif operacao == "relogio":
        nome = partes[1]
        hora = int(partes[2])
        resposta = atualizar_relogio(nome,hora)
    elif operacao == "eleicao":
        novo_coordenador = eleger_coordenador()
        resposta = (f"coordenador|{novo_coordenador}")

    socket.send(msgpack.packb(resposta))
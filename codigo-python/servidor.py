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


# Adicionando o relógio lógico
contador_logico = 0


# Função para atualizar o relógio lógico ao receber uma mensagem
def atualizar_relogio(recebido):
    global contador_logico
    contador_logico = max(contador_logico, recebido)
    return contador_logico

# Função para extrair relógio recebido
def extrair_relogio(valor):
    try:
        return int(valor.split(":")[1])
    except:
        return 0

# Configuração para comunicação com o serviço de referência
ref_socket = context.socket(zmq.REQ)
ref_socket.connect("tcp://referencia:5560")

# Contador de mensagens para heartbeat
contador_mensagens = 0

while True:
    mensagem_bin = socket.recv()
    mensagem = msgpack.unpackb(mensagem_bin, raw=False)
    lista_msg = mensagem.split("|")
    operacao = lista_msg[0]
    conteudo = lista_msg[1]
    timestamp = lista_msg[2]

    relogio_cliente = extrair_relogio(lista_msg[3])
    contador_logico = atualizar_relogio(relogio_cliente)
    contador_logico += 1

    if operacao == "login":
        if conteudo in usuarios:
            resposta = f"erro|relogio:{contador_logico}"
        else:
            resposta = f"login|relogio:{contador_logico}"
    elif operacao == "canais":
        if conteudo != "eof":
            saida_operacao = adiciona_canal(canais_recebidos, conteudo)
            resposta = f"sucesso|relogio:{contador_logico}"
        else:
            resposta = f"erro|relogio:{contador_logico}"
    elif operacao == "listar":
        resposta = saida_operacao
    elif operacao == "canal":
        lista_conteudo = conteudo.split("-")
        canal = lista_conteudo[0]
        mensagem_conteudo = lista_conteudo[1]
        pub.send_string(canal, flags=zmq.SNDMORE)
        pub.send_string(mensagem_conteudo)
        print(f"PUBLICANDO: {canal} | MSG:{mensagem_conteudo}")
        resposta = f"ok|relogio:{contador_logico}"
    else:
        resposta = f"erro inesperado|relogio:{contador_logico}"

    # Extrai o relógio enviado pelo cliente

    # Atualiza o relógio lógico

    # Incrementa o contador de mensagens e envia heartbeat se necessário
    # contador_mensagens += 1
    # if contador_mensagens >= 10:
    #     ref_socket.send(msgpack.packb({"operacao": "heartbeat", "conteudo": "servidor"}))
    #     resposta_ref = msgpack.unpackb(ref_socket.recv(), raw=False)
    #     print(f"Heartbeat enviado: {resposta_ref}", flush=True)
    #     contador_mensagens = 0


    resposta = resposta.strip().lower()
    print(f"{resposta}", flush=True)
    time.sleep(1)
    socket.send(msgpack.packb(resposta))

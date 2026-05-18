using Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using MessagePack;


class Program
{
    static string coordenador = "";

    static void Main(string[] args)
    {
        HashSet<string> loadedNames = ServerHelpers.BuildNamesList();
        HashSet<string> loadedChannels = new HashSet<string>();

        string nomeServidor = "servidor-csharp";
        int relogio_servidor = 0;
        int contadorMensagens = 0;

        using (var server = new ResponseSocket())
        using (var pubSocket = new PublisherSocket())
        using (var subSocket = new SubscriberSocket())
        using (var referenceSocket = new RequestSocket())
        {
            server.Connect("tcp://broker:5556");
            subSocket.Connect("tcp://proxy:5557");
            pubSocket.Connect("tcp://proxy:5558");
            referenceSocket.Connect("tcp://reference:5559");

            subSocket.Subscribe("server");
            Thread.Sleep(1000);

            string msgRegistro = $"registro|{nomeServidor}|{relogio_servidor}";
            string respostaRegistro = ServerHelpers.EnviaMsgReferencia(referenceSocket, msgRegistro);
            Console.WriteLine($"Resposta do registro: {respostaRegistro}");

            Task.Run(() =>
            {
                while (true)
                {
                    string topico = subSocket.ReceiveFrameString();
                    string mensagemSub = subSocket.ReceiveFrameString();

                    coordenador = mensagemSub;
                    Console.WriteLine($"RECEBENDO {topico} | MSG: {mensagemSub}");
                    Console.WriteLine($"Novo coordenador: {mensagemSub}");
                }
            });

            while (true)
            {
                string message, response, content, operation, time;

                message = ServerHelpers.GetMessage(server);
                operation = ServerHelpers.GetOperation(message);
                content = ServerHelpers.GetContent(message);
                relogio_servidor = ServerHelpers.GetClock(message, relogio_servidor);

                contadorMensagens++;

                switch (operation)
                {
                    case "login":
                        response = LoginAutication(content, loadedNames);
                        break;

                    case "canais":
                        response = ChannelsValidation(content, loadedChannels);
                        break;

                    case "listar":
                        response = ChannelsList(loadedChannels);
                        break;

                    case "canal":
                        response = PublishMessage(content, pubSocket);
                        break;

                    default:
                        response = "...";
                        break;
                }
                ServerHelpers.SendToClient(response, server, relogio_servidor);
                Thread.Sleep(1000);
                ServerHelpers.WriteMessage(message, response);

                if (contadorMensagens % 15 == 0)
                {
                    string msgHeartbeat = $"heartbeat|{nomeServidor}|{relogio_servidor}";
                    Console.WriteLine(msgHeartbeat);
                    ServerHelpers.EnviaMsgReferencia(referenceSocket, msgHeartbeat);

                    string msgRelogio = $"relogio|{nomeServidor}|{relogio_servidor}";
                    Console.WriteLine(msgRelogio);
                    string respostaRelogio = ServerHelpers.EnviaMsgReferencia(referenceSocket, msgRelogio);

                    if (respostaRelogio == "servidor_removido")
                    {
                        string msgEleicao = "eleicao||";
                        Console.WriteLine(msgEleicao);
                        string respostaEleicao = ServerHelpers.EnviaMsgReferencia(referenceSocket, msgEleicao);

                        string[] partesEleicao = respostaEleicao.Split('|');
                        if (partesEleicao.Length > 1)
                        {
                            coordenador = partesEleicao[1];
                            Console.WriteLine($"Novo coordenador {coordenador}");

                            if (coordenador == nomeServidor)
                            {
                                pubSocket.SendMoreFrame("server").SendFrame(nomeServidor);
                                Console.WriteLine($"PUBLICANDO: server | MSG: {nomeServidor}");
                            }
                        }
                    }
                    else
                    {
                        string[] partesRelogio = respostaRelogio.Split('|');
                        if (partesRelogio.Length > 1 && int.TryParse(partesRelogio[1], out int novaHora))
                        {
                            relogio_servidor = novaHora;
                            Console.WriteLine($"Relógio sincronizado {relogio_servidor}");
                        }
                    }
                }

                string msgListarServidores = "listar||";
                string listaServidores = ServerHelpers.EnviaMsgReferencia(referenceSocket, msgListarServidores);
                Console.WriteLine($"{listaServidores}");
            }
        }
    }

    static string PublishMessage(string content, PublisherSocket pubSocket)
    {
        string[] parts = content.Split('-', 2);

        string canal = parts[0].Trim();
        string mensagemCorpo = parts[1].Trim();

        pubSocket.SendMoreFrame(canal).SendFrame(mensagemCorpo);
        Console.WriteLine($"PUBLICANDO: {canal} | MSG: {mensagemCorpo}");
        Thread.Sleep(1000);

        return "ok";
    }

    static string ChannelsList(HashSet<string> loadedChannels)
    {
        string channelsList = "";
        int i = 0;

        foreach (string channel in loadedChannels)
        {
            string line = "";

            if (i > 0)
            {
                line += "\n";
            }
            line += $"Canal {i}: {channel}";
            channelsList += line;
            i++;
        }
        if (loadedChannels.Count() == 0)
        {
            channelsList += "Sem canais";
        }
        return channelsList;
    }

    static string ChannelsValidation(string channel, HashSet<string> loadedChannels)
    {
        if (channel != "eof")
        {
            loadedChannels.Add(channel);
            return "sucesso";
        }

        return "erro";
    }

    static string LoginAutication(string name, HashSet<string> loadedNames)
    {
        bool existentName = loadedNames.Add(name);
        if (existentName)
        {
            return "login";
        }
        return "erro";
    }
}

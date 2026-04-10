using Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;

class Program
{
    static void Main(string[] args)
    {
        HashSet<string> loadedNames = ServerHelpers.BuildNamesList();
        HashSet<string> loadedChannels = new HashSet<string>();

        using (var server = new ResponseSocket())
        using (var pubSocket = new PublisherSocket())
        {
            server.Connect("tcp://broker:5556");
            pubSocket.Connect("tcp://proxy:5558");
            while (true)
            {
                string message, response, content, operation, time;

                message = ServerHelpers.GetMessage(server);
                operation = ServerHelpers.GetOperation(message);
                content = ServerHelpers.GetContent(message);
                time = ServerHelpers.GetTime(message);

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
                ServerHelpers.SendToClient(response, server);
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

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
        {
            server.Bind("tcp://*:5555");
            while (true)
            {
                string message, response, content, operation, time;
                message = server.ReceiveFrameString();

                content = ServerHelpers.GetContent(message);
                operation = ServerHelpers.GetOperation(message);
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

                    default:
                        response = "...";
                        break;
                }
                ServerHelpers.SendToClient(response, server);
            }
        }
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

        return channelsList;
    }

    static string ChannelsValidation(string channel, HashSet<string> loadedChannels)
    {
        bool existentChannel = loadedChannels.Add(channel);

        if (existentChannel)
        {
            return "sucesso";
        }
        return "erro";
    }

    static string LoginAutication(string name, HashSet<string> loadedNames)
    {
        if (loadedNames.Contains(name))
        {
            return "erro";
        }
        return "login";
    }
}

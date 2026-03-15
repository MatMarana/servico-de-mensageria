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
    static HashSet<string> BuildNamesList()
    {
        string path = "names.txt";
        HashSet<string> loadedNames = new HashSet<string>();

        try
        {
            string content = File.ReadAllText(path);
            string[] names = content.Split(",");

            loadedNames = names.Select(n => n.Trim().ToLower()).OrderBy(x => Random.Shared.Next()).Take(7).ToHashSet();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
            return loadedNames;
        }

        Console.WriteLine("========================= | Nomes Carregados | =========================");
        foreach (string name in loadedNames)
        {
            Console.Write($"{name} | ");
        }
        Console.WriteLine("\n========================================================================");
        return loadedNames;
    }

    static void Main(string[] args)
    {
        HashSet<string> loadedNames = BuildNamesList();
        HashSet<string> loadedChannels = new HashSet<string>();

        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://*:5555");
            while (true)
            {
                string message, response, content, operation, time;
                message = server.ReceiveFrameString();

                content = GetContent(message);
                operation = GetOperation(message);
                time = GetTime(message);

                switch (operation)
                {
                    case "login":
                        response = LoginAutication(content, loadedNames);
                        break;

                    case "canais":
                        response = CanaisValidation(content, loadedChannels);
                        break;

                    default:
                        response = "...";
                        break;
                }

                Console.WriteLine(response);
                Thread.Sleep(500);
                server.SendFrame(response);
            }
        }
    }

    static string GetTime(string message)
    {
        return message.Split("|")[2].Trim().ToLower();
    }

    static string GetContent(string message)
    {
        return message.Split("|")[1].Trim().ToLower();
    }

    static string GetOperation(string message)
    {
        return message.Split("|")[0].Trim().ToLower();
    }

    static string LoginAutication(string name, HashSet<string> loadedNames)
    {
        if (loadedNames.Contains(name))
        {
            return "erro";
        }
        return "login";
    }

    static string CanaisValidation(string channel, HashSet<string> loadedChannels)
    {
        bool existentChannel = loadedChannels.Add(channel);

        if (existentChannel)
        {
            return "sucesso";
        }
        return "erro";
    }
}

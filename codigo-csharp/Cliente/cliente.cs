using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static void Main(string[] args)
    {
        string[] names = ReadFile("names.txt");
        string[] channels = ReadFile("channels.txt");

        string step = "login";

        using (var client = new RequestSocket())
        {
            client.Connect("tcp://servidor-csharp:5555");
            int i = 0;

            while (true)
            {
                string message;

                switch (step)
                {
                    case "login":
                        message = Login(names[i], client);
                        break;

                    case "canais":
                        message = Channels(channels, client);
                        break;

                    default:
                        message = "...";
                        break;
                }

                step = GetStep(message, step);
                Thread.Sleep(1000);
                i++;
            }
        }
    }

    static string Channels(string[] channels, RequestSocket client)
    {
        string channel, shipping, message;
        int index = Random.Shared.Next(0, channels.Length);

        channel = channels[index];
        shipping = FormatShipping("canais", channel);
        message = SendToServer(shipping, client);

        return message;
    }

    static string Login(string nome, RequestSocket client)
    {
        string message, shipping;

        shipping = FormatShipping("login", nome);
        message = SendToServer(shipping, client);

        return message;
    }

    static string[] ReadFile(string path)
    {
        string[] content = [];

        try
        {
            string file = File.ReadAllText(path);
            content = file.Split(", ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
        }
        return content;
    }

    static string GetStep(string message, string step)
    {
        if (step == "login" && message == "login")
        {
            return "canais";
        }

        if (step == "canais" && message == "erro")
        {
            return "listar";
        }
        return step;
    }

    static string FormatShipping(string operation, string content)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");
        return $"{operation}|{content}|{time}".ToLower();
    }

    static string SendToServer(string shipping, RequestSocket client)
    {
        string message;

        Console.WriteLine(shipping);
        Thread.Sleep(500);

        client.SendFrame(shipping);
        message = client.ReceiveFrameString().ToLower();

        return message;
    }
}

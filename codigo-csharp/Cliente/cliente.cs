using System;
using System.Threading;
using Utils;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static void Main(string[] args)
    {
        string[] names = ClientHelpers.ReadFile("names.txt");
        string[] channels = ClientHelpers.ReadFile("channels.txt");

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

                    case "listar":
                        message = ListChannels(client);
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

    static string ListChannels(RequestSocket client)
    {
        string shipping, message;

        shipping = ClientHelpers.FormatShipping("listar", "");
        message = ClientHelpers.SendToServer(shipping, client);

        return message;
    }

    static string Channels(string[] channels, RequestSocket client)
    {
        string channel, shipping, message;
        int index = Random.Shared.Next(0, channels.Length);

        channel = channels[index];
        shipping = ClientHelpers.FormatShipping("canais", channel);
        message = ClientHelpers.SendToServer(shipping, client);

        return message;
    }

    static string Login(string nome, RequestSocket client)
    {
        string message, shipping;

        shipping = ClientHelpers.FormatShipping("login", nome);
        message = ClientHelpers.SendToServer(shipping, client);

        return message;
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
}
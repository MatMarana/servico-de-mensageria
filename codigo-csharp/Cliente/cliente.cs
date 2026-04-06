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
            client.Connect("tcp://broker:5555");
            int namesIndex = 0, channelsIndex = 0;

            while (true)
            {
                string message;
                switch (step)
                {
                    case "login":
                        message = Login(names[namesIndex], client);
                        namesIndex++;
                        break;

                    case "canais":
                        message = Channels(channels[channelsIndex], client);
                        channelsIndex++;
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
            }
        }
    }

    static string ListChannels(RequestSocket client)
    {
        string shipping, message;

        shipping = ClientHelpers.FormatShipping("listar", "");
        message = ClientHelpers.SendToServer(shipping, client);

        return ClientHelpers.FormatChannelsList(message);
    }

    static string Channels(string channel, RequestSocket client)
    {
        string shipping, message;

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

        if (step == "listar")
        {
            return "...";
        }

        return step;
    }
}
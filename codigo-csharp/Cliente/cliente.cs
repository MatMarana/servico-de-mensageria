using System;
using System.Threading;
using Utils;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static void Main(string[] args)
    {
        string receivedChannels =  "", step = "login";
        string[] names = ClientHelpers.ReadFile("names.txt");
        string[] channels = ClientHelpers.ReadFile("channels.txt");

        int namesIndex = 0, channelsIndex = 0;

        using var subSocket = new SubscriberSocket();
        using var client = new RequestSocket();

        client.Connect("tcp://broker:5555");
        subSocket.Connect("tcp://proxy:5555");

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
                    message = ListChannels(client, receivedChannels);
                    break;

                case "subscribing":
                    message = Subscribing(subSocket, receivedChannels);
                    break;

                default:
                    message = "...";
                    break;
            }

            step = GetStep(message, step);
            Thread.Sleep(1000);
        }
    }

    static string Subscribing(SubscriberSocket subSocket, string receivedChannels)
    {
        string[] channelsList = receivedChannels.Split(",");
        string channel1, channel2, channel3;

        channel1 = channelsList[0];
        channel2 = channelsList[1];
        channel3 = channelsList[2];

        subSocket.Subscribe(channel1);
        subSocket.Subscribe(channel2);
        subSocket.Subscribe(channel3);

        return "";
    }

    static string ListChannels(RequestSocket client, string receivedChannels)
    {
        string shipping, message;

        shipping = ClientHelpers.FormatShipping("listar", "");
        message = ClientHelpers.SendToServer(shipping, client);

        receivedChannels = ClientHelpers.FormatChannelsList(message);
        return receivedChannels;
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
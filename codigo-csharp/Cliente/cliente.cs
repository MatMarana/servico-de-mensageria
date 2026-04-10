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
        string[] subscribedChannels = new string[3];

        int namesIndex = 0, channelsIndex = 0, incremento = 0;

        using var subSocket = new SubscriberSocket();
        using var client = new RequestSocket();

        client.Connect("tcp://broker:5555");
        subSocket.Connect("tcp://proxy:5557");

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
                    receivedChannels = ListChannels(client);
                    message = receivedChannels;
                    break;

                case "subscribing":
                    subscribedChannels = Subscribing(subSocket, receivedChannels);
                    message = "...";
                    break;

                case "message request":
                    message = MessageRequest(client, subscribedChannels, incremento, subSocket);
                    incremento++;
                    break;

                default:
                    message = "...";
                    break;
            }

            step = GetStep(message, step);
            Thread.Sleep(1000);
        }
    }

    static string MessageRequest(RequestSocket client, string[] subscribedChannels, int incremento, SubscriberSocket subSocket)
    {
        string shipping, message;
        Random random = new Random();

        int randomIndex = random.Next(0, 3);
        string mensagem = subscribedChannels[randomIndex] + "-" + "Rock N Roll " + incremento.ToString();
        shipping = ClientHelpers.FormatShipping("canal", mensagem);
        message = ClientHelpers.SendToServer(shipping, client);

        Thread.Sleep(100);

        if (subSocket.TryReceiveFrameString(out string topicoRecebido))
        {
            string conteudoRecebido = subSocket.ReceiveFrameString();
            Console.WriteLine($"Canal {topicoRecebido} | Msg {conteudoRecebido}");
        }

        return message;
    }

    static string[] Subscribing(SubscriberSocket subSocket, string receivedChannels)
    {
        string[] subscribedChannels = new string[3];
        string[] channelsList = receivedChannels.Split(",");

        subscribedChannels[0] = channelsList[0];
        subscribedChannels[1] = channelsList[1];
        subscribedChannels[2] = channelsList[2];

        subSocket.Subscribe(subscribedChannels[0]);
        subSocket.Subscribe(subscribedChannels[1]);
        subSocket.Subscribe(subscribedChannels[2]);

        return subscribedChannels;
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
            return "subscribing";
        }

        if (step == "subscribing")
        {
            return "message request";
        }

        return step;
    }
}
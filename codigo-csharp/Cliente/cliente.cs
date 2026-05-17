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
        string[] names = ["Henrique", "Tiago", "Mateus", "Leo", "Pedro", "Givas", "Kawan", "Albertini", "Robertinho", "Ale"];
        string[] channels = ["Kiss","Nightwish","RedHot","Black Sabbath","Pedra Leticia","Raimundos","CBR Jr", "EOF"];
        string[] subscribedChannels = new string[3];

        int namesIndex = 0, channelsIndex = 0, incremento = 0, relogio_cliente = 0;

        using var subSocket = new SubscriberSocket();
        using var client = new RequestSocket();

        client.Connect("tcp://broker:5555");
        subSocket.Connect("tcp://proxy:5557");

        while (true)
        {
            string message;
            relogio_cliente++;
            switch (step)
            {
                case "login":
                    message = Login(names[namesIndex % names.Length], client, ref relogio_cliente);
                    namesIndex++;
                    break;

                case "canais":
                    message = Channels(channels[channelsIndex % channels.Length], client, ref relogio_cliente);
                    channelsIndex++;
                    break;

                case "listar":
                    receivedChannels = ListChannels(client, ref relogio_cliente);
                    message = receivedChannels;
                    break;

                case "subscribing":
                    subscribedChannels = Subscribing(subSocket, receivedChannels);
                    message = "...";
                    break;

                case "message request":
                    message = MessageRequest(client, subscribedChannels, incremento, subSocket, ref relogio_cliente);
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

    static string MessageRequest(RequestSocket client, string[] subscribedChannels, int incremento, SubscriberSocket subSocket, ref int relogio_cliente)
    {
        string shipping, message;
        Random random = new Random();

        int randomIndex = random.Next(0, 3);
        string mensagem = subscribedChannels[randomIndex] + "-" + "Rock N Roll " + incremento.ToString();
        shipping = ClientHelpers.FormatShipping("canal", mensagem, ref relogio_cliente);
        message = ClientHelpers.SendToServer(shipping, client, ref relogio_cliente);

        Thread.Sleep(100);

        if (subSocket.TryReceiveFrameString(out string topicoRecebido))
        {
            string conteudoRecebido = subSocket.ReceiveFrameString();
            Console.WriteLine($"RECENDO: {topicoRecebido} | MSG: {conteudoRecebido}");
        }

        return message;
    }

    static string[] Subscribing(SubscriberSocket subSocket, string receivedChannels)
    {
        string[] channelsList = receivedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries);

        int quantidadeParaAssinar = Math.Min(channelsList.Length, 3);

        string[] subscribedChannels = new string[quantidadeParaAssinar];

        for (int i = 0; i < quantidadeParaAssinar; i++)
        {
            subSocket.Subscribe(channelsList[i]);
            subscribedChannels[i] = channelsList[i];
        }

        return subscribedChannels;
    }

    static string ListChannels(RequestSocket client, ref int relogio_cliente)
    {
        string shipping, message;

        shipping = ClientHelpers.FormatShipping("listar", "", ref relogio_cliente);
        message = ClientHelpers.SendToServer(shipping, client, ref relogio_cliente);

        return ClientHelpers.FormatChannelsList(message);
    }

    static string Channels(string channel, RequestSocket client, ref int relogio_cliente)
    {
        string shipping, message;

        shipping = ClientHelpers.FormatShipping("canais", channel, ref relogio_cliente);
        message = ClientHelpers.SendToServer(shipping, client, ref relogio_cliente);

        return message;
    }

    static string Login(string nome, RequestSocket client, ref int relogio_cliente)
    {
        string message, shipping;

        shipping = ClientHelpers.FormatShipping("login", nome, ref relogio_cliente);
        message = ClientHelpers.SendToServer(shipping, client, ref relogio_cliente);

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
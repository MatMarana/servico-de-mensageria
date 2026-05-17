using NetMQ;
using NetMQ.Sockets;
using MessagePack;
using System.Collections.Generic;
namespace Utils;
public class ClientHelpers
{
    public static string FormatChannelsList(string channels)
    {
        string channelsList = string.Join(",", channels
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(linha => linha.Split(':').Last().Trim()));

        return channelsList;
    }
    public static string FormatShipping(string operation, string content, int relogio_cliente)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");
        return $"{operation}|{content}|{time}|relogio: {relogio_cliente}".ToLower();
    }

    public static string SendToServer(string shipping, RequestSocket client, ref int relogio_cliente)
    {
        string message, message_raw;
        int relogio_servidor;

        Message shippingObj = new Message
        {
            message = shipping
        };
        byte[] binaryData = MessagePackSerializer.Serialize(shipping);

        Console.WriteLine($"{shipping}");
        Thread.Sleep(1000);

        client.SendFrame(binaryData);

        byte[] responseBytes = client.ReceiveFrameBytes();
        string responseObj = MessagePackSerializer.Deserialize<string>(responseBytes);
        message_raw = responseObj.ToLower();

        message = message_raw.Split("|")[0];
        relogio_servidor = int.Parse(message_raw.Split("|")[1].Split(":")[1].Trim());

        relogio_cliente = relogio_cliente > relogio_servidor ? relogio_cliente : relogio_servidor;

        return message;
    }

    public static string[] ReadFile(string path)
    {
        string[] content = [];

        try
        {
            string file = File.ReadAllText(path);
            content = file.Split(",");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
        }
        return content;
    }
}

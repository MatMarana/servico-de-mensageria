using NetMQ;
using NetMQ.Sockets;
using MessagePack;
namespace Utils;
public class ClientHelpers
{
    public static string FormatShipping(string operation, string content)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");
        return $"{operation}|{content}|{time}".ToLower();
    }

    public static string SendToServer(string shipping, RequestSocket client)
    {
        string message;

        Message shippingObj = new Message
        {
            message = shipping
        };
        byte[] binaryData = MessagePackSerializer.Serialize(shippingObj);

        Console.WriteLine($"{shipping}");
        Thread.Sleep(500);

        client.SendFrame(binaryData);

        byte[] responseBytes = client.ReceiveFrameBytes();
        Message responseObj = MessagePackSerializer.Deserialize<Message>(responseBytes);
        message = responseObj.message.ToLower();

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

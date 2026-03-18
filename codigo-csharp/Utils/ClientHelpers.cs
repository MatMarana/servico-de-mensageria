using NetMQ;
using NetMQ.Sockets;

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

        Console.WriteLine($"C-Sharp: {shipping}");
        Thread.Sleep(500);

        client.SendFrame(shipping);
        message = client.ReceiveFrameString().ToLower();

        return message;
    }

    public static string[] ReadFile(string path)
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
}

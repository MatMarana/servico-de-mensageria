using NetMQ;
using NetMQ.Sockets;
using MessagePack;
namespace Utils;
public class ServerHelpers
{
    public static void SendToClient(string response, ResponseSocket server)
    {
        Message shippingObj = new Message
        {
            message = response
        };
        byte[] binaryData = MessagePackSerializer.Serialize(shippingObj);

        Console.WriteLine($"{response}");
        Thread.Sleep(500);
        server.SendFrame(binaryData);
    }

    public static HashSet<string> BuildNamesList()
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

        return loadedNames;
    }

    public static string GetMessage(ResponseSocket server)
    {
        byte[] responseBytes = server.ReceiveFrameBytes();

        Message responseObj = MessagePackSerializer.Deserialize<Message>(responseBytes);

        return responseObj.message.ToLower();
    }
    public static string GetTime(string message)
    {
        return message.Split("|")[2].Trim().ToLower();
    }

    public static string GetContent(string message)
    {
        return message.Split("|")[1].Trim().ToLower();
    }

    public static string GetOperation(string message)
    {
        return message.Split("|")[0].Trim().ToLower();
    }
}
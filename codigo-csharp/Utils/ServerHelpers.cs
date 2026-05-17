using NetMQ;
using NetMQ.Sockets;
using MessagePack;
using System.Collections.Generic;
namespace Utils;
public class ServerHelpers
{
    public static void SendToClient(string response, ResponseSocket server, int relogio_servidor)
    {
        response = $"{response}| relogio: {relogio_servidor}";
        byte[] binaryData = MessagePackSerializer.Serialize(response);

        Console.WriteLine($"{response}");
        Thread.Sleep(1000);
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

        string responseObj = MessagePackSerializer.Deserialize<string>(responseBytes);

        return responseObj.ToLower();
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

    public static int GetClock(string message, int relogio_servidor)
    {
        int relogio_cliente = int.Parse(message.Split("|")[3].Split(":")[1].Trim().ToLower());
        int maior_relogio = relogio_cliente > relogio_servidor ? relogio_cliente : relogio_servidor;
        return maior_relogio++;
    }
}
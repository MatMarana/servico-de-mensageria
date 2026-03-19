using NetMQ;
using NetMQ.Sockets;

namespace Utils;
public class ServerHelpers
{
    public static void SendToClient(string response, ResponseSocket server)
    {
        Console.WriteLine($"C-Sharp: {response}");
        Thread.Sleep(500);
        server.SendFrame(response);
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

        PrintLoadedNames(loadedNames);
        return loadedNames;
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

    static void PrintLoadedNames(HashSet<string> loadedNames)
    {
        Console.WriteLine("========================= | Nomes Carregados - C-Sharp | =========================");
        foreach (string name in loadedNames)
        {
            Console.Write($"{name} | ");
        }
        Console.WriteLine("\n========================================================================");
    }
}
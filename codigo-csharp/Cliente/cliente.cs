using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static string[] GetNamesFromFile()
    {
        string path = "dbload.txt";
        string[] nomes = [];

        try
        {
            string conteudo = File.ReadAllText(path);
            nomes = conteudo.Split(", ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
        }
        return nomes;

    }
    static void Main(string[] args)
    {
        string[] nomes = GetNamesFromFile();
        using (var client = new RequestSocket())
        {
            client.Connect("tcp://servidor-csharp:5555");
            int i = 0;

            while (true)
            {
                string nome, message;
                if (i == 10)
                {
                    Console.WriteLine("Impossível Logar");
                    break;
                }

                nome = nomes[i];
                client.SendFrame($"Login: {nome}");
                message = client.ReceiveFrameString();

                Console.WriteLine(message);

                Thread.Sleep(1000);
                i++;
            }
        }
    }
}

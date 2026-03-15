using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static void Main(string[] args)
    {
        string[] nomes = GetNamesFromFile();
        string etapa = "Login";

        using (var client = new RequestSocket())
        {
            client.Connect("tcp://servidor-csharp:5555");
            int i = 0;

            while (true)
            {
                string mensagem;

                switch (etapa)
                {
                    case "Login":
                        mensagem = Login(nomes[i], client);
                        break;

                    case "Canal":
                        mensagem = "Canais";
                        break;

                    default:
                        mensagem = "...";
                        break;
                }

                etapa = GetEtapa(mensagem, etapa);
                Thread.Sleep(1000);
                i++;
            }
        }
    }

    static string Login(string nome, RequestSocket client)
    {
        string mensagem, horario, envio;

        horario = DateTime.Now.ToString("HH:mm:ss");
        envio = $"Login|{nome}|{horario}";

        Console.WriteLine(envio);
        Thread.Sleep(500);

        client.SendFrame(envio);
        mensagem = client.ReceiveFrameString();

        return mensagem;
    }

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

    static string GetEtapa(string mensagem, string etapa)
    {
        if (mensagem == "Login")
        {
            return "Canal";
        }

        return etapa;
    }
}

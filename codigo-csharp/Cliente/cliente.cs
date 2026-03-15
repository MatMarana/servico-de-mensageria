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
        string mensagem, horario;

        horario = DateTime.Now.ToString("HH:mm:ss");

        client.SendFrame($"Login|{nome}|{horario}");
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
        Console.WriteLine(mensagem);

        if (mensagem == "Autorizado")
        {
            return "Canal";
        }

        return etapa;
    }
}

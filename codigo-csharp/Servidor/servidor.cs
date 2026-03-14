using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static HashSet<string> BuildNamesList()
    {
        string path = "dbload.txt";
        HashSet<string> nomesCarregados = new HashSet<string>();

        try
        {
            string conteudo = File.ReadAllText(path);
            string[] nomes = conteudo.Split(",");

            nomesCarregados = nomes.Select(n => n.Trim()).OrderBy(x => Random.Shared.Next()).Take(9).ToHashSet();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler o arquivo: {ex.Message}");
            return nomesCarregados;
        }

        return nomesCarregados;
    }
    static void Main(string[] args)
    {
        HashSet<string> nomesCarregados = BuildNamesList();

        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://*:5555");
            while (true)
            {
                string message, resposta, conteudo, operacao;
                message = server.ReceiveFrameString();

                conteudo = GetContent(message);
                operacao = GetOperation(message);

                switch (operacao)
                {
                    case "Login":
                        resposta = LoginAutication(conteudo, nomesCarregados);
                        break;

                    default:
                        resposta = "...";
                        break;
                }
                server.SendFrame(resposta);
            }
        }
    }

    static string GetContent(string message)
    {
        return message.Split(":")[1].Trim();
    }

    static string GetOperation(string message)
    {
        return message.Split(":")[0].Trim();
    }
    static string LoginAutication(string nome, HashSet<string> nomesCarregados)
    {
        if (nomesCarregados.Contains(nome))
        {
            return "Recusado";
        }
        return "Autorizado";
    }
}

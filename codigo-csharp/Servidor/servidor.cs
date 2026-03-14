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
    static List<string> BuildNamesList()
    {
        string path = "dbload.txt";
        List<string> nomesCarregados = new List<string>();

        try
        {
            string conteudo = File.ReadAllText(path);
            string[] nomes = conteudo.Split(",");

            nomesCarregados = nomes.Select(n => n.Trim()).OrderBy(x => Random.Shared.Next()).Take(9).ToList();
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
        List<string> nomesCarregados = BuildNamesList();

        foreach (string nome in nomesCarregados)
        {
            Console.WriteLine(nome);
        }
        using (var server = new ResponseSocket())
        {
            int i = 0;
            server.Bind("tcp://*:5555");
            while (true)
            {
                string message = server.ReceiveFrameString();
                string name = nomesCarregados[i];
                server.SendFrame(name);

                i = i < 9 ? i = i + 1 : 8;
            }
        }
    }
}

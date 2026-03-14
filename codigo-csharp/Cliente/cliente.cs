using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
class Program
{
    static void Main(string[] args)
    {
        using (var client = new RequestSocket())
        {
            client.Connect("tcp://servidor-csharp:5555");
            int i = 0;

            while (true)
            {
                Console.Write($"Mensagem {i}: ");

                client.SendFrame("Hello");
                string message = client.ReceiveFrameString();
                Console.WriteLine(message);
                i++;

                Thread.Sleep(1000);
            }
        }
    }
}

using System;

namespace PicChat
{
    class Program
    {
        static void Main(string[] args)
        {
            Common.LoadData();
            Console.WriteLine("Hello World - Server Starting.");
            Common.server = new HttpServer();
            Common.server.StartServer(new string[] { "http://localhost:8080/" });
            var res = "";
            while(res != "quit")
            {
                res = Console.ReadLine();
                Common.server.MessageClients($"<div class='message'>!!!TEST {DateTime.Now} TEST!!</div>");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    public class HttpServer
    {
        public bool RunServer = true;
        public List<ClientConnection> Connections = new List<ClientConnection>();
        public void StartServer(string[] prefixes) {
            Thread thread = new Thread(()=> {
            
            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            Console.WriteLine("Listening...");
            while (RunServer)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                Connections.Add(new ClientConnection(context));
            }
            listener.Stop();
            listener.Close();
            });
            thread.Start();

        }

    }
}

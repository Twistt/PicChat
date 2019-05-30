using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace PicChat
{
    public class HttpServer
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);

        // Declare the event.
        public event MessageEventHandler MessageReceived;
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
                    if (context.Request.HttpMethod == "POST")
                    {
                        ProcessRequest(context);
                    }
                    else {
                        Connections.Add(new ClientConnection(context));
                    }

                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: New connection");
            }
            listener.Stop();
            listener.Close();
            });
            thread.Start();

        }
        public void MessageClients(string message) {
            MessageReceived(this, new MessageEventArgs(message));
        }
        private void ProcessRequest(HttpListenerContext context)
        {
            // Get the data from the HTTP stream
            var body = new StreamReader(context.Request.InputStream).ReadToEnd();
            var parts = body.Split("&");
            var uid = parts[1].Split("=")[1];
            var message = parts[0].Split("=")[1];
            Console.WriteLine(body);
            context.Response.Close();
            Common.server.MessageClients($"<div class='message'>{DateTime.UtcNow.ToShortTimeString()} {uid}: {WebUtility.UrlDecode(message)}</div>");
            //Console.WriteLine($"{}");
        }
    }
    public class MessageEventArgs
    {
        public MessageEventArgs(string s) { Text = s; }
        public String Text { get; } // readonly
    }
}

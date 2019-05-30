using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PicChat
{
    public class ClientConnection: IDisposable
    {
        Stream OutputStream;
        HttpListenerResponse response;


        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        //protected virtual void RaiseSampleEvent()
        //{
        //    // Raise the event by using the () operator.
        //    if (HttpServer.MessageReceived != null)
        //        HttpServer.MessageReceived(this, new MessageEventArgs("Hello"));
        //}
        public ClientConnection(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            response = context.Response;
            OutputStream = response.OutputStream;
            Common.server.MessageReceived += (o,e) =>
            {
                SendData(e.Text);
            };
            SendData(File.ReadAllText("templates/Main.html"));
        }
        public bool SendData(string data) {
            try
            {
                if (OutputStream.CanWrite)
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                    //response.ContentLength64 = buffer.Length;
                    OutputStream.Write(buffer);
                    OutputStream.Flush();
                    return true;
                }
            }
            catch (Exception err) {
                Console.WriteLine(err.Message);
                return false;
            }
            return true;
        }
        public void CloseConnection() {
            // You must close the output stream.
            OutputStream.Close();
            response.Close();
        }

        public void Dispose()
        {
            OutputStream.Close();
            response.Close();
            OutputStream = null;
            response = null;
        }
    }
}

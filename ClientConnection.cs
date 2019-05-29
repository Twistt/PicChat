using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ConsoleApp1
{
    public class ClientConnection
    {
        Stream OutputStream;
        HttpListenerResponse response;
        public ClientConnection(HttpListenerContext context) {
            HttpListenerRequest request = context.Request;
            response = context.Response;
            OutputStream = response.OutputStream;
        }
        public bool SendData(string data) {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                response.ContentLength64 = buffer.Length;
                OutputStream.Write(buffer, 0, buffer.Length);
                OutputStream.Flush();
                return true;
            }
            catch (Exception err) {
                Console.WriteLine(err.Message);
                return false;
            }
        }
        public void CloseConnection() {
            // You must close the output stream.
            OutputStream.Close();
            response.Close();
        }
    }
}

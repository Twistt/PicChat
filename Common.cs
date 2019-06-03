using Newtonsoft.Json;
using PicChat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PicChat
{
    public class Common
    {
        public static HttpServer server = null;
        public static Data data = new Data();
        public static string GenerateNeatHash(string tohash) {
            var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(tohash))).Replace("=", "").Replace("+", "").Replace("/", "");
        }
        public static void LoadData()
        {
            if (!File.Exists("Data.json")) return;
            var fdata = File.ReadAllText("Data.json");
            data = Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(fdata);

        }
        public static void SaveData()
        {
            var fdata = JsonConvert.SerializeObject(data);
            File.WriteAllText("Data.json",fdata);
        }
        public static void WriteFileData(string filename, HttpListenerContext context, Dictionary<string, string> valuePairs = null)
        {
            var fileContents = File.ReadAllText(filename);
            if (valuePairs != null)
            {
                foreach (var item in valuePairs)
                {
                    fileContents = fileContents.Replace(item.Key, item.Value);
                }
            }
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(fileContents);
            //response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer);
            //context.Response.OutputStream.Flush();
            context.Response.Close();
        }
    }
}

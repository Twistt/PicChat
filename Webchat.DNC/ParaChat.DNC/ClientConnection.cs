using ArachnidCreations.SqliteDAL;
using ParaChat.DNC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ParaChat.DNC
{
    public class ClientConnection
    {
        private HttpListenerContext context;
        HttpListenerRequest request= null;
        HttpListenerResponse response = null;
        Timer timer = null;
        public ChatUser User = null;
        public string UserID;
        public ClientConnection(HttpListenerContext context, ChatUser user)
        {
            try
            {
                Console.WriteLine("Accepted new client connection.");
                User = user;
                UserID = user.ID;
                this.context = context;
                timer = new Timer(TimerTick, null, 1000, 5000);
                context.Response.KeepAlive = true;
                request = context.Request;
                response = context.Response;
                SendMessage(null, new ChatMessage(File.ReadAllText("Templates/ChatTemplate.html").Replace("{{UserID}}", UserID.ToString())) { isParsed = true });
                Server.PublicChat += SendMessage;

                foreach (var message in Common.Messages.OrderByDescending(c => c.DATETIME).Take(500).Reverse().ToList())
                {
                    if (message.isPrivate && message.ToUser == User.UserName || message.FromUser == User.UserName || !message.isPrivate) SendMessage(null, message);
                }
            }
            catch (Exception) {
                Console.WriteLine("Unable to handle user");
            }
        }
        private void TimerTick(object state)
        {
            SendMessage(null, 
                new ChatMessage("<style type='text/css'>" +
                "#Users::after {content: '" + Common.Users.Count + "';} " +
                "#Messages::after {content: '" + Common.Messages.Count + "';} " +
                "#Library::after {content: '" + Common.Library.Count + "';} " +
                "#Online::after {content: '" + Common.Users.Where(u=>u.LoggedIn == true).Count() + "';}" +
                "#MyShares::after {content: '" + User.TotalShares + "';} " +
                "</style>") {UserName="System", isParsed=true });
        }

        public void SendMessage(object sender, ChatMessage message)
        {

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message.ParsedMessage);
            // Get a response stream and write the response to it.
            //response.ContentLength64 = buffer.Length;
            try
            {
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Flush();
            }
            catch (Exception err)
            {
                Console.WriteLine("Client connection closed for client #" + UserID);
                Common.ClientsToRemove.Add(this);
            }
            // You must close the output stream.
            //output.Close();
            
        }


        public void HandleMessage(Dictionary<string, string> data)
        {
            var message = WebUtility.UrlDecode(data["message"]);
            try
            {
                if (message.StartsWith("/")) {
                    var parts = message.Split(' ');
                    var prefix = parts[0].Replace("/", "");
                    if (parts.Length > 1)
                    {
                        if (prefix == "pm")
                        {
                            var client = Server.Clients.Where(u=>u.User.UserName == parts[1]).FirstOrDefault();
                            if (client != null)
                            {
                                var pos = message.IndexOf(parts[2]);

                                var privateMessage = new ChatMessage(message.Substring(pos, message.Length-pos)) { isPrivate = true, FromUser = User.UserName, ToUser = client.User.UserName, UserName = User.UserName };
                                if (!client.User.BlockedUsers.Contains(User.UserName))
                                    client.SendMessage(null, privateMessage);
                                SendMessage(null, privateMessage);
                                ORM.Insert<ChatMessage>(privateMessage);
                                Common.Messages.Add(privateMessage);
                            }
                        }
                        if (prefix == "search")
                        {
                            var criterion = parts[1].Replace("'", "").Replace(";", "").Replace("-", "").Trim();
                            if (User.HasLibraryCard)
                            {
                                
                                List<Book> books = ORM.convertDataTabletoObject<Book>(DAL.Load($"select * from Library where link like '%{criterion}%' or tags like '%{criterion}%' limit 25;"));
                            }
                            else {
                                var count = DAL.Load($"select count(*) from Library where link like '%{criterion}%' or tags like '%{criterion}%' limit 25;").Rows[0][0].ToString();
                                SendMessage(null, new ChatMessage($"You do not have have a library card to see the {count} messages your query returned.") { UserName = "Library" });

                            }
                        }
                    }
                    if (prefix == "help") SendMessage(null, new ChatMessage(CannedMessages.Help) { UserName = "System"});
                    if (prefix == "list") SendMessage(null, new ChatMessage(CannedMessages.UserList) { UserName = "System" });
                }
                else
                {
                    var chatmessage = new ChatMessage(message) { UserName = User.UserName };
                    ORM.Insert<ChatMessage>(chatmessage);
                    Common.Messages.Add(chatmessage);
                    //invoke chat event for all users (temporary until we get pms in).
                    Server.RaiseChatEvent(chatmessage);
                    Console.WriteLine(User.UserName + ":" + message);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(User.UserName + err.Message);
            }
        }
    }
}

using ArachnidCreations.SqliteDAL;
using ParaChat.DNC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ParaChat.DNC
{
    public static class Server
    {
        public static List<ClientConnection> Clients = new List<ClientConnection>();
        public static bool shouldAbort = false;
        public static event EventHandler<ChatMessage> PublicChat;
        public static Timer garbageTimer = new Timer(TimerTick);
        private static void TimerTick(object state) {
            var client = Common.ClientsToRemove.FirstOrDefault();
            if (client != null)
            {
                Clients.Remove(client);
            }
        }
        public static void RaiseChatEvent(ChatMessage chatmessage)
        {
            Server.PublicChat?.Invoke(null, chatmessage);
        }
        public static void RunServer() {
            // Create a listener.
            HttpListener listener = new HttpListener();
            var prefix = "http://127.0.0.1:3001/";
		// Add the prefixes.
            listener.Prefixes.Add(prefix);

            listener.Start();
            Console.WriteLine("Listening on " + prefix);
            // Note: The GetContext method blocks while waiting for a request. 
            while (!shouldAbort)
            {
                HttpListenerContext context = listener.GetContext();
                context.Response.ContentType = "text/html";
                if (context.Request.HttpMethod != "POST")
                {
                    if (IsBanned(null, context))
                    {
                        Console.WriteLine("attempted/rejected login from banned user.");
                        context.Response.Close();
                        return;
                    }
                    Cookie cookie = context.Request.Cookies["UserID"];
                    string id = string.Empty;
                    if (cookie != null)
                    {
                        id = cookie.Value;
                        ChatUser user = Common.Users.Where(u=>u.ID == id).FirstOrDefault();
                        if (user.isBanned)
                        {
                            context.Response.Close();
                            return;
                        }
                        var client = new ClientConnection(context, user);
                        Clients.Add(client);

                    }
                    else
                    {
                        //if they are not already logged in then show them the login page.
                        ReturnPage(id, context, "Templates/login.html");
                        context.Response.Close();
                    }
                    //This should only happen during a succesfull login 

                }
                else //Posted data
                {
                    //Parse post data
                    var body = new StreamReader(context.Request.InputStream).ReadToEnd().Split('&');
                    Dictionary<string, string> postData = new Dictionary<string, string>();
                    foreach (var item in body) postData.Add(item.Split('=')[0], item.Split('=')[1]);

                    if (IsBanned(postData,context))
                    {
                        context.Response.Close();
                        return;
                    }

                    if (context.Request.RawUrl.Contains("/send"))
                    {
                        var client = Clients.Where(c => c.UserID == postData["UserID"]).FirstOrDefault();
                        if (client != null)
                        {
                            client.HandleMessage(postData);
                        }
                        ReturnPage(postData["UserID"], context, "Templates/chatform.html");
                        context.Response.Close();
                    }

                    if (context.Request.RawUrl.Contains("/chat"))
                    {
                        if (postData.ContainsKey("register") && postData["register"].ToString() == "true")
                        {
                            if (!CreateAccount(postData, context)) context.Response.Close();
                            ChatUser user = new ChatUser() { UserName = postData["username"], Password = postData["password"], LoggedIn=true, ID = Guid.NewGuid().ToString() };

                            Common.Users.Add(user);
                            ORM.Insert<ChatUser>(user);
                        }

                        if (postData.ContainsKey("username"))
                        {
                            var username = postData["username"].ToString();
                            username  = username.Replace("'", "");
                            var user = Common.Users.Where(u=>u.UserName == username && u.Password == postData["password"]).FirstOrDefault();

                            if (user != null)
                            {
                                if (user.ID == string.Empty) user.ID = Guid.NewGuid().ToString();
                                user.LoggedIn = true;
                                Clients.Add(new ClientConnection(context, user));
                            }
                            else ReturnPage(string.Empty, context, "Templates/login.html", "Username/password combination not found");
                        }
                    }
                }
            }
            listener.Stop();
        }
        public static bool IsBanned(Dictionary<string, string> data, HttpListenerContext context) {
            if (data == null)
            {
                var res = context.Request.Cookies["paranoia_ban"];
                if (res != null) return true;
            }
            if (data == null) return false;
            if (!data.Keys.Contains("username")) return false;
            var username = data["username"];
            if (username != null && username.Contains("&nbsp"))
            {
                try
                {
                    context.Response.SetCookie(new Cookie("paranoia_ban", "true"));
                }
                catch (Exception er) { }
                return true;
            }
            return false;
        }
        public static bool CreateAccount(Dictionary<string, string> postData, HttpListenerContext context) {
            var username =postData["username"];
            if (username == null)
            {
                ReturnPage(string.Empty, context, "Templates/login.html", "You must choose a username.");
                return false;
            }
            if (username =="System" || username == "Library" || username == "Admin" || username == "Moderator" || username.Contains("<") || username.Contains(">") || username.ToLower() == "admin")
            {
                ReturnPage(string.Empty, context, "Templates/login.html", "This username is reserved and you cannot have it.");
                return false;
            }
            //var regex = /^[a-zA-Z0-9!@#\$%\^\&*\)\(+=._-]+$/g; 
            var okname = Regex.Match(username,@"/^[\w &.\-] *$/") == null ? false : true;
            var oklen = Regex.Match(username, "/^.{ 3,}$/") == null ? false : true;

            if (!okname || !oklen || username.Contains("&"))
            {
                Console.WriteLine(username + " - OK length?:", oklen, "OK chars?:", okname);

                ReturnPage(string.Empty, context, "Templates/login.html", "This username is invalid.");
                return false;
            }
          
            return true;
        }
        public static void ReturnPage(string userid, HttpListenerContext context, string pagename, string error = "" )
        {
            var retval = File.ReadAllText(pagename).Replace("{{UserID}}", userid.ToString()).Replace("{{error}}", error);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(retval);
            // Get a response stream and write the response to it.
            //response.ContentLength64 = buffer.Length;
            System.IO.Stream output = context.Response.OutputStream;
            try
            {
                output.Write(buffer, 0, buffer.Length);
                output.Flush();
            }
            catch (HttpListenerException err)
            {
                Console.WriteLine("Client connection closed for client #" + userid);

            }
        }
    }
}

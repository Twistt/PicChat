using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace PicChat
{
    public class HttpServer
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public List<string> Messages = new List<string>();
        // Declare the event.
        public event MessageEventHandler MessageReceived;
        public bool RunServer = true;
        public List<ClientConnection> Connections = new List<ClientConnection>();

        public void StartServer(string[] prefixes)
        {
            Thread thread = new Thread(() =>
            {
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
                    string UID = string.Empty;

                    if (context.Request.RawUrl.EndsWith(".jpg") || context.Request.RawUrl.EndsWith(".png"))
                    {
                        var buffer = File.ReadAllBytes("images/" + context.Request.RawUrl.Split("/").LastOrDefault());
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer);
                        context.Response.Close();
                        continue;
                    }
                    if (context.Request.RawUrl.EndsWith(".ico"))
                    {
                        context.Response.Close();
                        continue;
                    }

                    // Did the request come with a cookie?
                    Cookie cookie = context.Request.Cookies["UID"];
                    if (cookie != null)
                    {
                        UID = cookie.Value;
                    }
                    if (UID != null && UID != string.Empty)
                    {
                        Console.WriteLine(UID);
                    }
                    if (context.Request.HttpMethod == "GET")
                    {
                        UID = Common.GenerateNeatHash(DateTime.UtcNow.ToString() + DateTime.UtcNow.Millisecond.ToString());
                        Cookie cook = new Cookie("UID", UID);
                        context.Response.AppendCookie(cook);
                        Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                        context.Response.Close();
                        continue;
                    }


                    if (context.Request.HttpMethod == "POST")
                    {
                        if (IsBanned(null, context))
                        {
                            Console.WriteLine("attempted/rejected login from banned user.");
                            context.Response.Close();
                            return;
                        }
                        ProcessRequest(context, UID);
                    }
                    //else
                    //{
                    //    if (context.Request.RawUrl.EndsWith("login"))
                    //    {
                    //        Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                    //    }
                    //    else
                    //    {
                    //        if (UID != null && UID != string.Empty)
                    //        {
                    //            ClientConnection conn = null;
                    //            var user = Common.data.Users.Where(u => u.UID == UID).FirstOrDefault();
                    //            if (user == null)
                    //            {
                    //                Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                    //                context.Response.Close();
                    //                return;
                    //            }
                    //            else
                    //            {
                    //                conn = Connections.Where(c => c.User.UID == UID).FirstOrDefault();
                    //                Connections.Remove(conn);
                    //                Connections.Add(new ClientConnection(context, user));
                    //            }
                    //        }
                    //    }
                    //}
                }
                Console.WriteLine("Listener Stopped! Crap.");
                listener.Stop();
                listener.Close();
            });
            thread.Start();

        }
        public void MessageClients(string message)
        {
            MessageReceived(this, new MessageEventArgs(message));
        }
        private void ProcessRequest(HttpListenerContext context, string UID)
        {
            HttpListenerRequest request = context.Request;

            // Get the data from the HTTP stream
            var body = System.Web.HttpUtility.UrlDecode(new StreamReader(context.Request.InputStream).ReadToEnd()).Split('&');
            Dictionary<string, string> postData = new Dictionary<string, string>();
            foreach (var item in body) postData.Add(item.Split('=')[0], item.Split('=')[1]);

            var userConn = Connections.Where(u => u.User.UID == UID).FirstOrDefault();
            var login = postData.GetValueOrDefault("username");

            var action = postData.GetValueOrDefault("action");

            if (action == "login")
            {
                var pass = postData.GetValueOrDefault("password");

                if (login == null || login.EndsWith("="))
                {
                    Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                    return;
                }

                var existingUser = Common.data.Users.Where(u => u.UserName == login).FirstOrDefault();
                if (existingUser != null && existingUser.PasswordHash == pass)
                {
                    var cc = new ClientConnection(context, new User { Joined = DateTime.UtcNow, UID = UID, UserName = login, PasswordHash=pass });
                    Connections.Add(cc);
                    return;
                }
                else if (existingUser == null)
                {
                    if (CreateAccount(postData, context))
                    {
                        var newuser = new User { Joined = DateTime.UtcNow, UID = UID, UserName = login, PasswordHash=pass };

                        var cc = new ClientConnection(context, newuser);
                        Connections.Add(cc);
                        Common.data.Users.Add(newuser);
                        Common.SaveData();
                        return;

                    }
                }
                else
                {
                    Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "That account already exists and this password is not correct" } });
                    return;
                }
                
                
                return;
            }

            //if (action == "showlarge")
            //{

            //    var pid = postData.GetValueOrDefault("pid");
            //    var post = Common.data.Posts.Where(p => p.PostID == pid).FirstOrDefault();

            //    if (post != null)
            //    {
            //        var conn = Connections.Where(c => c.User.UID == (UID)).FirstOrDefault();
            //        if (conn != null) conn.SendData(@"<style type='text/css'>.largePicView {display:inline; background-image:url('" + System.Web.HttpUtility.UrlDecode(post.URL) + "'); }</style>");
            //    }

            //}

            if (action == "hidelarge")
            {
                var conn = Connections.Where(c => c.User.UID == (UID)).FirstOrDefault();
                if (conn != null) conn.SendData(@"<style type='text/css'>.largePicView {display:none;}</style>");
            }
            if (action == "showimage")
            {
                var pid = postData.GetValueOrDefault("pid");
                var post = Common.data.Posts.Where(p => p.PostID == pid).FirstOrDefault();

                if (post != null)
                {
                    var conn = Connections.Where(c => c.User.UID == (UID)).FirstOrDefault();
                    if (conn != null) conn.SendData(@"<style type='text/css'>." + pid + "_picContainer {background-image:url('" + System.Web.HttpUtility.UrlDecode(post.URL) + "')} ." + pid + "_volatile {display:none;} ." + pid + "_largebutton {display:inline;}</style>");
                }
            }

            if(action == "post")
            {
                
                var message = postData.GetValueOrDefault("message");
                var url = postData.GetValueOrDefault("url");
                var repeat = Common.data.Posts.Where(p => p.URL == url).FirstOrDefault();
                if (repeat != null)
                {
                    message = "[Repost]" + message;
                }
                context.Response.Close();
                Post post = new Post() { Message = message, UID = UID, TimeStamp = DateTime.UtcNow, URL = url };
                var contents = File.ReadAllText("templates/Post.html").Replace("{uid}", UID).Replace("{url}", url).Replace("{username}", userConn.User.UserName).Replace("{pid}", post.PostID).Replace("{message}", post.Message);
                Common.server.MessageClients(contents);
                Common.data.Posts.Add(post);
                Common.SaveData();
            }
            //Console.WriteLine($"{}");
        }
        public static bool IsBanned(Dictionary<string, string> data, HttpListenerContext context)
        {
            if (data == null)
            {
                var res = context.Request.Cookies["Pictochat_ban"];
                if (res != null) return true;
            }
            if (data == null) return false;
            if (!data.Keys.Contains("username")) return false;
            var username = data["username"];
            if (username != null && username.Contains("&nbsp"))
            {
                try
                {
                    context.Response.SetCookie(new Cookie("Pictochat_ban", "true"));
                }
                catch (Exception er) { }
                return true;
            }
            return false;
        }
        public static bool CreateAccount(Dictionary<string, string> postData, HttpListenerContext context)
        {
            var username = postData["username"];
            if (username == null)
            {
                Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "You must choose a username." } });
                ReturnPage(string.Empty, context, "Templates/login.html", "You must choose a username.");
                return false;
            }
            if (username == "System" || username == "Library" || username == "Admin" || username == "Moderator" || username.Contains("<") || username.Contains(">") || username.ToLower() == "admin")
            {
                Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "This username is reserved and you cannot have it." } });
                return false;
            }
            //var regex = /^[a-zA-Z0-9!@#\$%\^\&*\)\(+=._-]+$/g; 
            var okname = Regex.Match(username, @"/^[\w &.\-] *$/") == null ? false : true;
            var oklen = Regex.Match(username, "/^.{ 3,}$/") == null ? false : true;

            if (!okname || !oklen || username.Contains("&"))
            {
                Console.WriteLine(username + " - OK length?:", oklen, "OK chars?:", okname);
                Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "This username is invalid." } });
                return false;
            }

            return true;
        }
        public static void ReturnPage(string userid, HttpListenerContext context, string pagename, string error = "")
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
    public class MessageEventArgs
    {
        public MessageEventArgs(string s) { Text = s; }
        public String Text { get; } // readonly
    }
}

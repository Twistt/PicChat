using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                        Console.WriteLine("Found the cookie!");
                    }
                    if (UID == null || UID == string.Empty)
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
                        ProcessRequest(context, UID);
                    }
                    else
                    {
                        if (context.Request.RawUrl.EndsWith("login"))
                        {
                            Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                        }
                        else
                        {
                            if (UID != null && UID != string.Empty)
                            {
                                ClientConnection conn = null;

                                conn = Connections.Where(c => c.User.UID == UID).FirstOrDefault();
                                if (conn != null)
                                {
                                    //Take over previous connection with this connection information with cookie.
                                    conn.response = context.Response;
                                    conn.OutputStream = context.Response.OutputStream;
                                }
                                else
                                {
                                    Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                                }
                            }

                        }
                    }

                    //Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: New connection");
                }
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
            var body = new StreamReader(context.Request.InputStream).ReadToEnd();
            var parts = body.Split("&");
            var action = "";
            var userConn = Connections.Where(u => u.User.UID == UID).FirstOrDefault();

            var oaction = parts.Where(p => p.StartsWith("action")).FirstOrDefault();
            if (oaction != null) action = oaction.Split("=")[1];
            if (action == "login")
            {

                var login = parts.Where(p => p.StartsWith("username")).FirstOrDefault();
                if (login == null || login.EndsWith("="))
                {
                    Common.WriteFileData("templates/Login.html", context, new Dictionary<string, string> { { "{loginresults}", "" } });
                    return;
                }

                var cc = new ClientConnection(context, new User { Joined=DateTime.UtcNow, UID=UID, UserName= login.Split("=")[1] });
                //ToDo: login script.
                Connections.Add(cc);
                return;
            }

            if (action == "showlarge")
            {

                    var pid = System.Web.HttpUtility.UrlDecode(parts.Where(p => p.StartsWith("pid")).FirstOrDefault().Split("=")[1]);
                    var post = Common.data.Posts.Where(p => p.PostID == pid).FirstOrDefault();

                    if (post != null)
                    {
                        var conn = Connections.Where(c => c.User.UID == (UID)).FirstOrDefault();
                        if (conn != null) conn.SendData(@"<style type='text/css'>.largePicView {background-image:url('" + System.Web.HttpUtility.UrlDecode(post.URL) + "'); display:block; position:absolute; top:10%; left:10%;}</style>");
                    }

            }
            if (action == "showimage")
            {
                var pid = System.Web.HttpUtility.UrlDecode(parts.Where(p => p.StartsWith("pid")).FirstOrDefault().Split("=")[1]);
                var post = Common.data.Posts.Where(p => p.PostID == pid).FirstOrDefault();

                if (post != null)
                {
                    var conn = Connections.Where(c => c.User.UID == (UID)).FirstOrDefault();
                    if (conn != null) conn.SendData(@"<style type='text/css'>." + pid + "_picContainer {background-image:url('" + System.Web.HttpUtility.UrlDecode(post.URL) + "')} ." + pid + "_volatile {display:none;} ." + pid + "_largebutton {display:inline;}</style>");
                }
            }
            if(action == "post")
            {
                var message = parts.Where(p => p.StartsWith("message")).FirstOrDefault().Split("=")[1];
                var url = parts.Where(p => p.StartsWith("url")).FirstOrDefault().Split("=")[1];
                context.Response.Close();
                Post post = new Post() { Message = message, UID = UID, TimeStamp = DateTime.UtcNow, URL = url };
                var contents = File.ReadAllText("templates/Post.html").Replace("{uid}", UID).Replace("{username}", userConn.User.UserName).Replace("{pid}", post.PostID).Replace("{message}", post.Message);
                Common.server.MessageClients(contents);
                Common.data.Posts.Add(post);
            }
            //Console.WriteLine($"{}");
        }

    }
    public class MessageEventArgs
    {
        public MessageEventArgs(string s) { Text = s; }
        public String Text { get; } // readonly
    }
}

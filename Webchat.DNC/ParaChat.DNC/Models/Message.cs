using ArachnidCreations.SqliteDAL;
using ParaChat.DNC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParaChat.DNC
{
    [DBTable("Messages")]
    public class ChatMessage
    {
        public ChatMessage()
        {
        }
        public ChatMessage(string Text) {
            Message = Text;
            DATETIME = DateTime.Now;
        }
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string ParsedMessage { get {
                if (isParsed) return Message;

                else
                {
                    var classes = $"user_{UserName}";
                    var messagePrefix = "";
                    if (UserName == "System") classes += " systemMessage";
                    else classes += " message";
                    if (UserName == "Pictochat") classes += " PictochatPink";
                    if (UserName == "Library") classes += " libraryBlue";

                    if (isPrivate)
                    {
                        classes += " privateMessage";
                        messagePrefix = $":: <span>[From {FromUser} to {ToUser}]</span> :: ";

                    }
                    
                    ChatUser user = Common.Users.Where(u => u.UserName == UserName).FirstOrDefault();
                    if (user == null && UserName == "System") user = new ChatUser() { UserName = UserName, ID="SYSTEM" };
                    if (user == null && UserName == "Library") user = new ChatUser() { UserName = UserName, ID = "LIBRARY" };
                    ParseLinks();
                    SaveToLibrary();
                    foreach (var link in LinkList)
                    {
                        ParsedLinkList.Add($"<a href='{link}' target='_blank'>{link.Split('/').Last()}</a>");
                    }
                    return $"<div class='{classes}'>{messagePrefix} {DATETIME.ToShortTimeString()} - {(user.HasLibraryCard ? " <span style='color:gold;'>&copy;</span> " : "")} " +
                        $"<span>{UserName}</span>: {Message} {String.Join('|', TagList)} {String.Join('|',ParsedLinkList.ToArray())}</div>";
                }
            } }
        public string ParseLinks() {
            if (Message.Contains("http"))
            {
                //get tags out first
                var m = Message.Split(" ");
                for (var i = 0; i < m.Length; i++)
                {
                    var item = m[i].Trim();
                    if (item == "") continue;
                    if (item.Contains("http"))
                    {
                        if (item.EndsWith("/")) item = item.TrimEnd('/');
                        Message = Message.Replace(item, "").Trim();
                        LinkList.Add(item);
                    }
                    else
                    {
                        if (TagList.Contains(item)) continue;
                        if (item.Contains("["))
                        {
                            TagList.Add(item);
                        }
                        //to add things that are not bracketed - leave out for now.
                        //else
                        //{
                        //    if (TagList.Contains("[" + item.Trim() + "]")) continue;
                        //    TagList.Add("[" + item.Trim() + "]");
                        //}
                    }
                }
            }
            return Message;
        }
        private void SaveToLibrary() {
            if (LinkList.Count > 0)
            {

                foreach (var link in LinkList)
                {
                    var book = new Book();
                    book.Link = link;
                    book.Tags = String.Join('|', TagList);
                    book.UserName = UserName;
                    book.ShareDate = DateTime.UtcNow;
                    //me.Library.push(book);
                    var existing = Common.Library.Where(l => l.Link == link).FirstOrDefault();
                    if (existing == null)
                    {
                        Console.WriteLine("Added post by " + book.UserName + " to library.");
                        var user = Common.Users.Where(u => u.UserName == UserName).FirstOrDefault();
                        user.TotalShares++;
                        ORM.Insert<Book>(book);
                        Common.Library.Add(book);
                        ORM.Update<ChatUser>(user);
                    } 
                }
                //todo mark resposts as reposts
            }
        }
        public DateTime DATETIME { get; set; }
        public string Links { get; set; }
        public List<string> LinkList = new List<string>();
        public List<string> ParsedLinkList = new List<string>();
        public string Tags { get; set; }
        public List<string> TagList = new List<string>();
        public bool isPrivate { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public bool Hidden { get; set; }
        public bool isParsed { get; set; }

    }
}

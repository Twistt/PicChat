using ArachnidCreations.SqliteDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParaChat.DNC.Models
{
    [DBTable("Users")]
    public class ChatUser
    {
        [DBPrimaryKey]
        public int rowid { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ID { get; set; }
        public List<ChatMessage> Messages = new List<ChatMessage>();
        public List<string> BlockedUsers = new List<string>();
        public List<Book> MyLibrary = new List<Book>();
        public int TotalShares { get; set; }
        public int TotalMessages { get; set; }
        public bool HasLibraryCard { get; set; }
        public bool LoggedIn { get; set; }
        public bool isBanned { get; set; }
        public bool isCurator { get; set; }
        public string LastMessage { get; set; }
    }
}

using ParaChat.DNC.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParaChat.DNC
{
    public static class Common
    {
        public static List<ChatMessage> Messages = new List<ChatMessage>();
        public static List<ChatUser> Users = new List<ChatUser>();
        public static List<Book> Library = new List<Book>();
        public static List<ClientConnection> ClientsToRemove = new List<ClientConnection>();

    }
}

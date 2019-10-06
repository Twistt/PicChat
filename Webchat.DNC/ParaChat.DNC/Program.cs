using ArachnidCreations.SqliteDAL;
using ParaChat.DNC.Models;
namespace ParaChat.DNC
{
    class Program
    {
        public static void Main(string[] args)
        {
            DAL.DBFile = "ParaChat.db";
            DoDataCleanup();
            //Console.Write(res);
            DAL.Exec("Update users set loggedin = 0;");
            Common.Library = ORM.convertDataTabletoObject<Book>(DAL.Load("Select * from Library;"));
            Common.Messages = ORM.convertDataTabletoObject<ChatMessage>(DAL.Load("Select * from Messages order by rowid desc limit 1000;"));
            Common.Users = ORM.convertDataTabletoObject<ChatUser>(DAL.Load("Select * from Users;"));
            Server.RunServer();
        }
        public static void DoDataCleanup() {
            //sharedate is not a valid date format

            //var dates =
            //var messagelist = DAL.Load("select id, username, message, cast(timestamp as text) as DATETIME, links, tags, isprivate, hidden from messages limit 1000;");
            //List<ChatMessage> messages = ORM.convertDataTabletoObject<ChatMessage>(messagelist);

        }
    }
}

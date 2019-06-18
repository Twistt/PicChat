using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParaChat.DNC
{
    public static class CannedMessages
    {
        public static string Help { get {
            var messageText = "Thank you for using Paranoia Chat! <br /> <span style='color:red;'>{ALPHA v1.2}</span><h3>Rules:</h3> <ul>";
                messageText += "<li>Sharing: Users who SHARE content will be ranked up - the blue number next to a poster's name <span style='color:lightblue;'>(99)</span> is indicative of their rank (based on shares)</li>";
                messageText += "<li>Libary: Users who SHARE content and rank up will be given access to the Library (the Library is a <i>searchable</i> database of images)</li>";
                messageText += "<li>Content: Any content goes as long as it is tagged with [tagname].</li>";
                messageText += "<li>Bans: if 3 Library Card holders vote to ban you you will be kicked/blocked and your library points reset to Zero.</li>";
                messageText += "</ul > <br /> Working commands are as follows / help, /users, /list, /library, /search {keywords -eg girl, blah} /block { user }, /pm {name} {message}";
            return messageText;
        } }
        public static string UserList
        {
            get {
                var retval = "Logged In Users - ";
                foreach (var user in Common.Users.Where(u => u.LoggedIn).ToList())
                {
                    retval +=(user.UserName + " ");
                }
                return retval;
            }
        }
    }
}

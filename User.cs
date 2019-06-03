using System;
using System.Collections.Generic;
using System.Text;

namespace PicChat
{
    public class User
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public DateTime Joined { get; set; }


    }
}

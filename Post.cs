using System;
using System.Collections.Generic;
using System.Text;

namespace PicChat
{
    public class Post
    {

        public Post() {
            PostID = Common.GenerateNeatHash(DateTime.UtcNow.ToString() + DateTime.UtcNow.Millisecond.ToString() + UID);
        }
        public string PostID { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }
        public string UID { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}

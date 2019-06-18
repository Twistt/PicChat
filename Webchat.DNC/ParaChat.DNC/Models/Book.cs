using ArachnidCreations.SqliteDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParaChat.DNC.Models
{
    [DBTable("Library")]

    public class Book
    {
        public string Link { get; set; }
        public string Tags { get; set; }
        public List<string> TagList = new List<string>();
        public string UserName { get; set; }
        public DateTime ShareDate { get; set; }
    }
}

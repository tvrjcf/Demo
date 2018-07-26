using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo_Http.models
{
    public class heiyan_chapter
    {
        public chapter chapter { get; set; }
        public string code { get; set; }
        public string hasHongBao { get; set; }
        public string nextChapterId { get; set; }
        public string preChapterId { get; set; }
    }

    public class chapter
    {
        public string additional { get; set; }
        public book book { get; set; }
        public bool free { get; set; }
        public string htmlContent { get; set; }
        public int id { get; set; }
        public string showTime { get; set; }
        public int status { get; set; }
        public string title { get; set; }
        public string words { get; set; }
        //public writerVO writerVO { get; set; }
    }

    public class book
    {
        public string iconUrlMedium { get; set; }
        public int id { get; set; }
        public string title { get; set; }
    }
}

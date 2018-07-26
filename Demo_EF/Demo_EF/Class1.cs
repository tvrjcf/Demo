using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo_EF
{
    public class Blog
     {
         public int BlogId { get; set; }
         public string Code { get; set; }
         public string Name { get; set; }
 
         public virtual List<Post> Posts { get; set; }
     }
     public class Post
     {
         public int PostId { set; get; }
         public string Title { get; set; }
         public string Content { get; set; }
 
         public int BlogId { set; get; }
         public virtual Blog Blog { get; set; }
     }

   
}

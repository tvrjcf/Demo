using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Demo_EF
{
    public class BlogContext : DbContext
    {
        public BlogContext()
            : base("name=DEMO_EF")
        {
        }
        public DbSet<Blog> Blogs { set; get; }
        public DbSet<Post> Posts { get; set; }
    }
}

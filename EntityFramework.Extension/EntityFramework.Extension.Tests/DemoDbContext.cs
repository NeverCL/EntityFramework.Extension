using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Tests
{
    public class DemoDbContext : BaseDbContext
    {
        static DemoDbContext()
        {
            GenerateViews(new DemoDbContext());
        }

        public DemoDbContext():base()
        {
            
        }

        public IDbSet<User> Users{ get; set; }
    }

    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}

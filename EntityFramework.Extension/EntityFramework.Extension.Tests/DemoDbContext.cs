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
            MasterSlave("Data Source=(localdb)\\test;Initial Catalog=Demo;Integrated Security=True;");
            GenerateViews(new DemoDbContext());
        }

        /// <summary>
        /// 当前线程DbContext
        /// </summary>
        public static DemoDbContext CurrentDb => CurrentDbContext<DemoDbContext>();


        public IDbSet<User> Users{ get; set; }
    }

    public class User
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}

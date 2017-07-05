using System.Data.Entity;
using EntityFramework.Extension.Entity;

namespace EntityFramework.Extension.Tests
{
    public class DemoDbContext : BaseDbContext
    {
        static DemoDbContext()
        {
            GenerateViews(new DemoDbContext());
        }

        /// <summary>
        /// 当前线程DbContext
        /// </summary>
        public static DemoDbContext CurrentDb => DbContextHelper.CurrentDbContext<DemoDbContext>();

        public IDbSet<User> Users { get; set; }
    }

    public class User : BaseEntity
    {

        public string Name { get; set; }
    }
}

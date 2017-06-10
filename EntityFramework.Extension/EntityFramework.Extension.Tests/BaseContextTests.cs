using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Extension.Tests
{
    [TestClass]
    public class BaseContextTests
    {
        public BaseContextTests()
        {
            InitData();
        }

        public void InitData()
        {
            var db = new DemoDbContext();
            db.Users.Add(new User
            {
                Name = "name"
            });
            db.SaveChanges(); 
        }

        /// <summary>
        /// 测试热处理
        /// </summary>
        [TestMethod]
        public void TestHotSelectMethod()
        {
            // 查询
            var users = new DemoDbContext().Users.ToList();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Extension.Tests
{
    [TestClass]
    public class BaseContextTests
    {
        public BaseContextTests()
        {
            //InitData();
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

        [TestMethod]
        public void TestSelectMethod()
        {
            var user1 = DemoDbContext.CurrentDb.Users.FirstOrDefault();
            var user2 = DemoDbContext.CurrentDb.Database.ExecuteSqlCommand("select * from Users");
            var user3 = DemoDbContext.CurrentDb.Database.SqlQuery<User>("select * from Users").ToList().FirstOrDefault();
            //Assert.IsTrue(!string.IsNullOrEmpty(user.Name));
        }


        /// <summary>
        /// 测试热处理 + 线程数据库
        /// </summary>
        [TestMethod]
        public void TestHotSelectMethod()
        {
            // 查询
            // 0.00.853   0.00.837   
            // 0:01.143   0:01.054   0:01.168   
            // 0:03.630
            for (int i = 0; i < 1000; i++)
            {
                DemoDbContext.CurrentDb.Users.ToList();
            }
        }

        /// <summary>
        /// 无缓存读取
        /// </summary>
        [TestMethod]
        public void TestNoCacheSelectMethod()
        {
            var user1 = DemoDbContext.CurrentDb.Users.FirstOrDefault();
            DemoDbContext.CurrentDb.Database.ExecuteSqlCommand("update Users set name = '" + Guid.NewGuid() + "' where id = 1");
            user1 = DemoDbContext.CurrentDb.Users.FirstOrDefault();
            var user2 = DemoDbContext.CurrentDb.Users.AsNoTracking().FirstOrDefault();
            Assert.IsTrue(user1.Name != user2.Name);
        }

        /// <summary>
        /// 无锁读取
        /// </summary>
        [TestMethod]
        public void TestNoLockRead()
        {
            // begin tran ..
            var nolockList = DemoDbContext.CurrentDb.NoLockFunc(() => DemoDbContext.CurrentDb.Users.ToList());
            // commit tran ..
            //Assert.IsTrue(list.Count != nolockList.Count);
        }




    }
}

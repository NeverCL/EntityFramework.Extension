using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.AspNet.Identity;

namespace EntityFramework.Extension
{
    public class BaseDbContext : DbContext
    {
        #region ctor
        static BaseDbContext()
        {
        }

        protected BaseDbContext() : this("DefaultConnection")
        {

        }

        protected BaseDbContext(DbConnection connection) : base(connection, true)
        {
        }

        protected BaseDbContext(string connStr)
            : base(connStr)
        {
        }

        /// <summary>
        /// EF 热处理
        /// </summary>
        /// <param name="db"></param>
        protected static void GenerateViews(DbContext db)
        {
            using (db)
            {
                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
            }
        }
        #endregion

        #region Thread
        /// <summary>
        /// 线程级 缓存 数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CurrentDbContext<T>() where T : DbContext, new()
        {
            var name = typeof(T).FullName;
            var db = CallContext.GetData(name) as T;
            if (db == null)
            {
                db = new T();
                CallContext.SetData(name, db);
            }
            return db;
        }
        #endregion

        #region HandleState
        protected virtual void ApplyConcepts()
        {
            foreach (DbEntityEntry entry in this.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        HandleAdd(entry);
                        continue;
                    case EntityState.Deleted:
                        HandleDelete(entry);
                        continue;
                    case EntityState.Modified:
                        HandleModify(entry);
                        if (entry.Entity is ISoftDelete && (entry.Entity as ISoftDelete).IsDeleted)
                        {
                            HandleDelete(entry);
                        }
                        continue;
                    default:
                        HandleSelect(entry);
                        LogManager.GetLogger(GetType()).Debug(entry);
                        continue;
                }
            }
        }

        protected virtual void HandleSelect(DbEntityEntry entry)
        {
            HandleState.Select(entry);
        }

        protected virtual void HandleAdd(DbEntityEntry entry)
        {
            HandleState.Add(entry);
        }

        protected virtual void HandleDelete(DbEntityEntry entry)
        {
            HandleState.Delete(entry);
        }

        protected virtual void HandleModify(DbEntityEntry entry)
        {
            HandleState.Modify(entry);
        }
        #endregion

        #region SaveChanges
        public override int SaveChanges()
        {
            ApplyConcepts();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                ApplyConcepts();
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                LogDbEntityValidationException(ex);
                throw;
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                ApplyConcepts();
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (DbEntityValidationException ex)
            {
                LogDbEntityValidationException(ex);
                throw;
            }
        }
        #endregion

        #region MasterSlave
        /// <summary>
        /// 主从复制
        /// </summary>
        /// <param name="slavedbConn"></param>
        protected static void MasterSlave(string slavedbConn)
        {
            DbInterception.Add(new DbMasterSlaveCommandInterceptor(slavedbConn));
        }
        #endregion

        #region NoLockFunc
        /// <summary>
        /// 事务期间读取 和 修改 可变数据
        /// 主要用于nolock读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T NoLockFunc<T>(Func<T> func)
        {
            var transactionOptions = new System.Transactions.TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            };
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required, transactionOptions))
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(GetType()).Error(ex);
                    throw;
                }
            }
        }
        #endregion

        #region Log
        protected virtual void LogDbEntityValidationException(DbEntityValidationException exception)
        {
            LogManager.GetLogger(GetType()).Error(exception);
        }
        #endregion
    }
}

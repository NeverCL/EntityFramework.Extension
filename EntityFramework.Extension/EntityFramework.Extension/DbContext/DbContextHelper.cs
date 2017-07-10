using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Transactions;
using Common.Logging;
using EntityFramework.Extension.Entity;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace EntityFramework.Extension
{
    public static class DbContextHelper
    {
        #region UpdateField
        /// <summary>
        /// 更新指定字段
        /// 忽略其他锁 直接更新字段
        /// 兼容对象存在上下文处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <param name="isSame"></param>
        /// <param name="propertyNames"></param>
        public static void UpdateField<T, TDbContext>(this TDbContext dbContext, T entity, Func<T, bool> isSame, params string[] propertyNames) where TDbContext : DbContext, new() where T : class
        {
            var db = CurrentDbContext<TDbContext>();
            db.Entry(entity).State = EntityState.Detached;
            var attachedEntity = db.Set<T>().Local.SingleOrDefault(isSame);
            if (attachedEntity != null)
            {
                // 对象存在上下文中
                var attachedEntry = db.Entry(attachedEntity);
                attachedEntry.CurrentValues.SetValues(entity);
            }
            else
            {
                // 对象不存在上下文中
                UpdateField(dbContext, entity, propertyNames);
            }
        }

        /// <summary>
        /// 更新指定字段
        /// 忽略其他锁 直接更新字段
        /// 当对象存在上下文会报错
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <param name="propertyNames"></param>
        public static void UpdateField<T, TDbContext>(this TDbContext dbContext, T entity, params string[] propertyNames) where TDbContext : DbContext, new() where T : class
        {
            var db = CurrentDbContext<TDbContext>();
            db.Set<T>().Attach(entity);
            var setEntry = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager.GetObjectStateEntry(entity);
            foreach (var propertyName in propertyNames)
            {
                setEntry.SetModifiedProperty(propertyName);
            }
        }

        /// <summary>
        /// 更新指定字段
        /// </summary>
        /// <typeparam name="TPrimaryKey"></typeparam>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity">实现 IEntity《TPrimaryKey》的实体</param>
        /// <param name="propertyNames"></param>
        public static void UpdateEntityField<TPrimaryKey, TDbContext>(this TDbContext dbContext, IEntity<TPrimaryKey> entity, params string[] propertyNames)
            where TDbContext : DbContext, new()
        {
            var db = CurrentDbContext<TDbContext>();
            db.Entry(entity).State = EntityState.Detached;
            var type = entity.GetType();
            var attachedEntity = db.Set(type).Find(entity.Id);
            if (attachedEntity != null)
            {
                // 对象存在上下文中
                var attachedEntry = db.Entry(attachedEntity);
                attachedEntry.CurrentValues.SetValues(entity);
            }
            else
            {
                // 对象不存在上下文中
                UpdateField(dbContext, entity, propertyNames);
            }
        }
        #endregion

        #region Thread
        /// <summary>
        /// 线程级 缓存 数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TDbContext CurrentDbContext<TDbContext>() where TDbContext : DbContext, new()
        {
            var name = typeof(TDbContext).FullName;
            var db = CallContext.GetData(name) as TDbContext;
            if (db == null)
            {
                db = new TDbContext();
                CallContext.SetData(name, db);
            }
            return db;
        }
        #endregion

        #region GenerateViews
        /// <summary>
        /// EF 热处理
        /// </summary>
        /// <param name="db"></param>
        public static void GenerateViews(this DbContext db)
        {
            var objectContext = ((IObjectContextAdapter)db).ObjectContext;
            var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            mappingCollection.GenerateViews(new List<EdmSchemaError>());
        }
        #endregion

        #region NoLockFunc
        /// <summary>
        /// 事务期间读取 和 修改 可变数据
        /// 主要用于nolock读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T NoLockFunc<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, T> func) where TDbContext : DbContext
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
            using (new TransactionScope(TransactionScopeOption.Required, transactionOptions))
            {
                try
                {
                    return func(dbContext);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(dbContext.GetType()).Error(ex);
                    throw;
                }
            }
        }
        #endregion

        #region TransExecute
        /// <summary>
        /// 如果存在环境事务，直接取环境事务，如果不存在，则创建新的事务执行
        /// 省略事务提交步骤
        /// </summary>
        public static T TransExecute<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, T> func, TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required) where TDbContext : DbContext
        {
            using (var trans = new TransactionScope(transactionScopeOption))
            {
                var rst = func(dbContext);
                trans.Complete();
                return rst;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
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
        protected BaseDbContext(): this("DefaultConnection")
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

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            ApplyConcepts();
            return base.SaveChanges();
        }

        private void ApplyConcepts()
        {
            foreach (DbEntityEntry entry in this.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetCreationAuditProperties(entry);
                        continue;
                    case EntityState.Deleted:
                        HandleSoftDelete(entry);
                        continue;
                    case EntityState.Modified:
                        SetModificationAuditProperties(entry);
                        if (entry.Entity is ISoftDelete && (entry.Entity as ISoftDelete).IsDeleted)
                        {
                            HandleSoftDelete(entry);
                        }
                        continue;
                    default:
                        HandleSelect();
                        continue;
                }
            }
        }

        private void HandleSelect()
        {
            throw new NotImplementedException();
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

        protected virtual void LogDbEntityValidationException(DbEntityValidationException exception)
        {
            LogManager.GetLogger(GetType()).Error(exception);
        }

        protected virtual string GetUserId()
        {
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (claimsPrincipal == null)
                return null;
            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
                return null;
            return claimsIdentity.GetUserId();
        }


        private void SetModificationAuditProperties(DbEntityEntry entry)
        {
            if (entry.Entity is IModificationAudited)
            {
                var entity = entry.Entity as IModificationAudited;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = GetUserId();
            }
        }
        private void HandleSoftDelete(DbEntityEntry entry)
        {
            if (entry.Entity is ISoftDelete)
            {
                entry.State = EntityState.Unchanged;
                var entity = entry.Entity as ISoftDelete;
                entity.IsDeleted = true;
                if (entry.Entity is IDeletionAudited)
                {
                    var deletionEntity = entry.Entity as IDeletionAudited;
                    deletionEntity.DeletionTime = DateTime.Now;
                    deletionEntity.DeleterUserId = GetUserId();
                }
            }
        }
        private void SetCreationAuditProperties(DbEntityEntry entry)
        {
            if (entry.Entity is ICreatorEntity)
            {
                var entity = entry.Entity as ICreatorEntity;
                entity.CreateTime = DateTime.Now;
                entity.CreatorId = GetUserId();
            }
        }
    }
}

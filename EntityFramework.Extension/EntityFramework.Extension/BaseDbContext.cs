using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using EntityFramework.Extension.Entity;
using EntityFramework.Extension.Interceptor;

namespace EntityFramework.Extension
{
    public class BaseDbContext : DbContext
    {
        #region ctor
        static BaseDbContext()
        {
            MasterSlave();
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
                        HandleDefault(entry);
                        LogManager.GetLogger(GetType()).Debug("HandleDefault" + entry);
                        continue;
                }
            }
        }

        protected virtual void HandleDefault(DbEntityEntry entry)
        {
            HandleState.Default(entry);
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
        protected static void MasterSlave()
        {
            DbInterception.Add(new DbMasterSlaveCommandInterceptor());
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

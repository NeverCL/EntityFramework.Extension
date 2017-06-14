using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNet.Identity;

namespace EntityFramework.Extension.Entity
{
    /// <summary>
    /// 处理状态
    /// </summary>
    public class HandleState
    {
        public static void Add(DbEntityEntry entry)
        {
            if (entry.Entity is ICreatorEntity)
            {
                var entity = entry.Entity as ICreatorEntity;
                entity.CreateTime = DateTime.Now;
                entity.CreatorId = GetUserId();
            }
        }

        public static void Delete(DbEntityEntry entry)
        {
            if (entry.Entity is ISoftDelete)
            {
                entry.State = EntityState.Unchanged;
                var entity = entry.Entity as ISoftDelete;
                entity.IsDeleted = true;
                if (entry.Entity is IDeletionEntity)
                {
                    var deletionEntity = entry.Entity as IDeletionEntity;
                    deletionEntity.DeletionTime = DateTime.Now;
                    deletionEntity.DeleterUserId = GetUserId();
                }
            }
        }

        public static void Modify(DbEntityEntry entry)
        {
            if (entry.Entity is IModifyEntity)
            {
                var entity = entry.Entity as IModifyEntity;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = GetUserId();
            }
        }

        public static void Default(DbEntityEntry entry)
        {

        }

        private static string GetUserId()
        {
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (claimsPrincipal == null)
                return null;
            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
                return null;
            return claimsIdentity.GetUserId();
        }
    }
}

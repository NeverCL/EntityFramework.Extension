using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace EntityFramework.Extension
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
                if (entry.Entity is IDeletionAudited)
                {
                    var deletionEntity = entry.Entity as IDeletionAudited;
                    deletionEntity.DeletionTime = DateTime.Now;
                    deletionEntity.DeleterUserId = GetUserId();
                }
            }
        }

        public static void Modify(DbEntityEntry entry)
        {
            if (entry.Entity is IModificationAudited)
            {
                var entity = entry.Entity as IModificationAudited;
                entity.LastModificationTime = DateTime.Now;
                entity.LastModifierUserId = GetUserId();
            }
        }

        public static void Select(DbEntityEntry entry)
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

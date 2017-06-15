using System;

namespace EntityFramework.Extension.Entity
{
    public class AuditionEntity<T> : BaseEntity<T>, IAuditionEntity
    {
        public DateTime CreateTime { get; set; }

        public string CreatorId { get; set; }

        public DateTime LastModificationTime { get; set; }

        public string LastModifierUserId { get; set; }

        public DateTime DeletionTime { get; set; }

        public string DeleterUserId { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class AuditionEntity : AuditionEntity<long>
    { }
}

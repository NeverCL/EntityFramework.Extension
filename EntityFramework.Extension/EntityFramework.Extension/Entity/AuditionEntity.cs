using System;

namespace EntityFramework.Extension.Entity
{
    public class AuditionEntity : IAuditionEntity
    {
        public DateTime CreateTime { get; set; }
        public string CreatorId { get; set; }
        public DateTime LastModificationTime { get; set; }
        public string LastModifierUserId { get; set; }
    }
}

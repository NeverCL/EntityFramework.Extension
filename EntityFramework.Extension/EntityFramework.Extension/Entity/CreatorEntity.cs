using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Entity
{
    public class CreatorEntity<T> : BaseEntity<T>, ICreatorEntity
    {
        public DateTime CreateTime { get; set; }
        public string CreatorId { get; set; }
    }

    public class CreatorEntity : CreatorEntity<long>
    {

    }
}

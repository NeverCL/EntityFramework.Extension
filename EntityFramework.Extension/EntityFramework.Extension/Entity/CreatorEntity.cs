using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Entity
{
    public class CreatorEntity : ICreatorEntity
    {
        public DateTime CreateTime { get; set; }
        public string CreatorId { get; set; }
    }
}

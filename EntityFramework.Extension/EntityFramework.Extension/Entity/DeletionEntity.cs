using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Entity
{
    public class DeletionEntity : IDeletionEntity
    {
        public DateTime DeletionTime { get; set; }
        public string DeleterUserId { get; set; }
    }
}

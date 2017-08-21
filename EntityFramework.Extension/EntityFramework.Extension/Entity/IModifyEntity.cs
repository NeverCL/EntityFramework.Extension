using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Entity
{
    public interface IModifyEntity
    {
        DateTime? LastModificationTime { get; set; }

        string LastModifierUserId { get; set; }
    }
}

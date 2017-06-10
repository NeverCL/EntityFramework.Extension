using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension
{
    public interface IModificationAudited
    {
        DateTime LastModificationTime { get; set; }

        string LastModifierUserId { get; set; }
    }
}

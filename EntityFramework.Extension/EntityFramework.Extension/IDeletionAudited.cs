using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension
{
    public interface IDeletionAudited
    {
        DateTime DeletionTime { get; set; }
        string DeleterUserId { get; set; }
    }
}

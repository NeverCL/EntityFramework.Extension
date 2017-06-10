using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension
{
    public interface ICreatorEntity
    {
        DateTime CreateTime { get; set; }

        string CreatorId { get; set; }
    }
}

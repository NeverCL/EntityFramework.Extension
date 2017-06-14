using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extension
{
    public static class EntityFrameworkConfig
    {
        /// <summary>
        /// 从库读开关
        /// </summary>
        public static bool IsSlaveRead = true;

        ///// <summary>
        ///// 线程缓存从库连接
        ///// 提示：当前使用这个性能有些低
        ///// TestMultiSelect [0:11.767] Success
        ///// </summary>
        //static bool IsThreadSlave = false;

        /// <summary>
        /// 从库连接字符串
        /// </summary>
        public static string ReadConnstr = "Data Source=(localdb)\\test;Initial Catalog=Demo;Integrated Security=True;";
    }
}

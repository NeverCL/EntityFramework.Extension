using System.Configuration;

namespace EntityFramework.Extension.Config
{
    public sealed class EntityFrameworkConfig : ConfigurationSection
    {
        /// <summary>
        /// 从库读开关
        /// </summary>
        [ConfigurationProperty("IsSlaveRead", DefaultValue = "false")]
        public bool IsSlaveRead
        {
            get { return (bool)this["IsSlaveRead"]; }
            set { this["IsSlaveRead"] = value; }
        }

        /// <summary>
        /// 从库连接字符串
        /// </summary>
        [ConfigurationProperty("ReadConnstr", DefaultValue = "")]
        public string ReadConnstr
        {
            get { return (string)this["ReadConnstr"]; }
            set { this["ReadConnstr"] = value; }
        }
    }
}

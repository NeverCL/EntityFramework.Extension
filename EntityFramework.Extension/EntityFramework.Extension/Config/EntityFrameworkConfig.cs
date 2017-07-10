using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EntityFramework.Extension.Config
{
    /// <summary>
    /// EF Config
    /// </summary>
    public sealed class EntityFrameworkConfig : ConfigurationSection
    {
        /// <summary>
        /// 主库连接字符串配置名
        /// </summary>
        [ConfigurationProperty("MasterConnName", DefaultValue = "DefaultConnection")]
        public string MasterConnName
        {
            get { return this["MasterConnName"].ToString(); }
            set { this["MasterConnName"] = value; }
        }

        /// <summary>
        /// 从库读开关
        /// </summary>
        [ConfigurationProperty("isSlaveRead", DefaultValue = "false")]
        public bool IsSlaveRead
        {
            get { return (bool)this["isSlaveRead"]; }
            set { this["isSlaveRead"] = value; }
        }

        /// <summary>
        /// 从库连接字符串
        /// </summary>
        [ConfigurationProperty("readConnstr")]
        public string ReadConnstr
        {
            get
            {
                return (string)this["readConnstr"];
            }
            set { this["readConnstr"] = value; }
        }

        /// <summary>
        /// 从库连接
        /// </summary>
        [ConfigurationProperty("slaves", IsDefaultCollection = true)]
        public ReaderConnectionCollection ReaderConnections
        {
            get { return (ReaderConnectionCollection)base["slaves"]; }
        }
    }


    public class ReaderConnectionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReaderConnection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ReaderConnection)element).Name;
        }

        protected override string ElementName { get { return "slaves"; } }

        /// <summary>
        /// 获取所有键
        /// </summary>
        public IEnumerable<string> AllKeys { get { return BaseGetAllKeys().Cast<string>(); } }


        public new ReaderConnection this[string name]
        {
            get { return (ReaderConnection)BaseGet(name); }
        }
    }


    public class ReaderConnection : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("connectionString")]
        public string ConnectionString
        {
            get { return (string)this["connectionString"]; }
            set { this["connectionString"] = value; }
        }

        [ConfigurationProperty("providerName", DefaultValue = "System.Data.SqlClient")]
        public string ProviderName
        {
            get { return (string)this["providerName"]; }
            set { this["providerName"] = value; }
        }

        [ConfigurationProperty("weight", DefaultValue = 1)]
        public int Weight
        {
            get { return (int)this["weight"]; }
            set { this["weight"] = value; }
        }
    }
}

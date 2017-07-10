using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using Common.Logging;
using EntityFramework.Extension.Config;

namespace EntityFramework.Extension.Interceptor
{
    public class DbMasterSlaveCommandInterceptor : DbCommandInterceptor
    {
        const string Typename = "DbMasterSlaveCommandInterceptor";

        //private readonly string _masterConnectionString = @"Data Source=(localdb)\test;Initial Catalog=masterdb;Integrated Security=True;";
        private readonly string _masterConnectionString;

        private readonly EntityFrameworkConfig _config;

        #region weight
        private List<string> _listWeight;
        private int maxWeight = -1;
        #endregion

        public DbMasterSlaveCommandInterceptor() : this((EntityFrameworkConfig)ConfigurationManager.GetSection("entityFrameworkConfig"))
        {

        }

        public DbMasterSlaveCommandInterceptor(EntityFrameworkConfig config)
        {
            this._config = config;
            _masterConnectionString = ConfigurationManager.ConnectionStrings[config.MasterConnName].ConnectionString;
            LogManager.GetLogger(Typename).Debug("DbMasterSlaveCommandInterceptor()");
        }

        #region 切换数据库链接

        void ChangeToReadConnectionString(DbInterceptionContext interceptionContext)
        {
            lock ("IsReadThreadSlave")
            {
                string connectionString;
                if (!string.IsNullOrEmpty(_config.ReadConnstr))
                    connectionString = _config.ReadConnstr;
                else
                    connectionString = GetWeightConnectString(_config.ReaderConnections);
                UpdateConnectionString(interceptionContext, connectionString);
            }
        }

        void ChangeToWriteConnectionString(DbCommandInterceptionContext<int> interceptionContext)
        {
            lock ("IsWriteThreadSlave")
            {
                UpdateConnectionString(interceptionContext, this._masterConnectionString);
            }
        }

        /// <summary>
        /// 修改数据库连接
        /// </summary>
        /// <param name="interceptionContext"></param>
        /// <param name="connectionString"></param>
        private void UpdateConnectionString(DbInterceptionContext interceptionContext, string connectionString)
        {
            foreach (var context in interceptionContext.DbContexts)
            {
                //this.UpdateConnectionString(context.Database.Connection, connectionString);
                this.UpdateConnectionStringIfNeed(context.Database.Connection, connectionString);
            }
        }

        /// <summary>
        /// 获取权重连接
        /// </summary>
        /// <param name="configReaderConnections"></param>
        /// <returns></returns>
        private string GetWeightConnectString(ReaderConnectionCollection configReaderConnections)
        {
            if (maxWeight == -1)
            {
                maxWeight = 0;
                _listWeight = new List<string>();
                foreach (ReaderConnection configReaderConnection in configReaderConnections)
                {
                    maxWeight += configReaderConnection.Weight;
                    for (int i = 0; i < configReaderConnection.Weight; i++)
                    {
                        _listWeight.Add(configReaderConnection.Name);
                    }
                }
            }
            var value = new Random().Next(maxWeight);
            var key = _listWeight[value];
            var connectionString = configReaderConnections[key].ConnectionString;
            return connectionString;
        }

        /// <summary>
        /// 根据需要 修改数据库连接
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="connectionString"></param>
        private void UpdateConnectionStringIfNeed(DbConnection conn, string connectionString)
        {
            if (!this.ConnectionStringCompare(conn, connectionString))
            {
                ConnectionState state = conn.State;
                if (state == ConnectionState.Open)
                    conn.Close();

                conn.ConnectionString = connectionString;

                if (state == ConnectionState.Open)
                    conn.Open();
            }
        }

        /// <summary>
        /// 判断数据库连接 是否一致
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="connectionString"></param>
        /// <returns>true：一致</returns>
        private bool ConnectionStringCompare(DbConnection conn, string connectionString)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(conn);

            DbConnectionStringBuilder a = factory.CreateConnectionStringBuilder();
            a.ConnectionString = conn.ConnectionString;

            DbConnectionStringBuilder b = factory.CreateConnectionStringBuilder();
            b.ConnectionString = connectionString;

            return a.EquivalentTo(b);
        }
        #endregion

        /// <summary>
        /// Linq 生成的select,insert + Database.SqlQuery<User>("select * from Users").ToList();
        /// prompt:在select语句中DbCommand.Transaction为null，而ef会为每个insert添加一个DbCommand.Transaction进行包裹
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            if (_config.IsSlaveRead && !command.CommandText.StartsWith("insert", StringComparison.CurrentCultureIgnoreCase) && command.Transaction == null)
            {
                ChangeToReadConnectionString(interceptionContext);
            }
            LogManager.GetLogger(Typename).Debug(command.CommandText);
        }

        /// <summary>
        /// Linq 生成的update,delete + Database.ExecuteSqlCommand
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            ChangeToWriteConnectionString(interceptionContext);
            LogManager.GetLogger(Typename).Debug(command.CommandText);
        }

        /// <summary>
        /// 执行sql语句，并返回第一行第一列，没有找到返回null,如果数据库中值为null，则返回 DBNull.Value
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            ChangeToReadConnectionString(interceptionContext);
            LogManager.GetLogger(Typename).Debug(command.CommandText);
        }
    }

}

using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using Common.Logging;
using EntityFramework.Extension.Config;

namespace EntityFramework.Extension.Interceptor
{
    public class DbMasterSlaveCommandInterceptor : DbCommandInterceptor
    {
        const string Typename = "DbMasterSlaveCommandInterceptor";

        private readonly EntityFrameworkConfig _config;

        public DbMasterSlaveCommandInterceptor() : this((EntityFrameworkConfig)ConfigurationManager.GetSection("entityFrameworkConfig"))
        {

        }

        public DbMasterSlaveCommandInterceptor(EntityFrameworkConfig config)
        {
            this._config = config;
            LogManager.GetLogger(Typename).Debug("DbMasterSlaveCommandInterceptor()");
        }

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
                ChangeReadConn(command.Connection);
            }
            LogManager.GetLogger(Typename).Debug(command.CommandText);
            base.ReaderExecuting(command, interceptionContext);
        }

        /// <summary>
        /// 修改读链接
        /// </summary>
        /// <param name="commandConnection"></param>
        private void ChangeReadConn(DbConnection commandConnection)
        {
            lock ("IsThreadSlave")
            {
                commandConnection.Close();
                if (_config.ReadConnstr != null)
                {
                    commandConnection.ConnectionString = _config.ReadConnstr;
                }
                // todo 权重算法取连接
                commandConnection.Open();
            }
        }

        /// <summary>
        /// Linq 生成的update,delete + Database.ExecuteSqlCommand
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogManager.GetLogger(Typename).Debug(command.CommandText);
            base.NonQueryExecuting(command, interceptionContext);
        }

        /// <summary>
        /// 执行sql语句，并返回第一行第一列，没有找到返回null,如果数据库中值为null，则返回 DBNull.Value
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogManager.GetLogger(Typename).Debug(command.CommandText);
            base.ScalarExecuting(command, interceptionContext);
        }

        public override void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            base.ReaderExecuted(command, interceptionContext);
        }

        public override void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            base.NonQueryExecuted(command, interceptionContext);
        }

        public override void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            base.ScalarExecuted(command, interceptionContext);
        }
    }

}

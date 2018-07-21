using MySql.Data.MySqlClient;
using PetaPoco.NetCore;
using System.Data.Common;
using System.Data.SqlClient;

namespace Timer.Web.Core.TableInit
{

    /// <summary>
    /// 初始化表
    /// </summary>
    public static class TableInit
    {



        public static bool IsInit = false;

        /// <summary>
        /// 单用户快速分析日志表(,mysql)
        /// </summary>
        private static string mysql_single_user_analysis_history_info = @"
CREATE TABLE if not exists  `single_user_analysis_history_info`  (
  `id` int(10) UNSIGNED NOT NULL  AUTO_INCREMENT COMMENT '自增列',
  `task_detail_id` int(10) NULL COMMENT '工单id',
  `col_create_time` timestamp(0) NULL,
  `spend_time` int(10) NULL DEFAULT 0,
  `step_no` int(10) NULL DEFAULT 0,
  `step_name` varchar(255) NULL DEFAULT '',
  `step_remark` varchar(255) NULL DEFAULT '',
  `error_msg` varchar(1024) NULL DEFAULT '',
  `run_status` varchar(50) NULL DEFAULT '',
  PRIMARY KEY (`id`)
);";

        /// <summary>
        /// 单用户快速分析日志表(sqlserver)
        /// </summary>
        private static string sqlserver_single_user_analysis_history_info = @"
CREATE TABLE [dbo].[single_user_analysis_history_info] (
  [id] int IDENTITY(1,1) NOT NULL,
  [task_detail_id] bigint DEFAULT 0 NOT NULL,
  [col_create_time] datetime NOT NULL,
  [spend_time] int DEFAULT 0 NOT NULL,
  [step_no] int DEFAULT 0 NULL,
  [step_name] varchar(255) DEFAULT '' NULL,
  [step_remark] varchar(255) DEFAULT '' NULL,
  [error_msg]  varchar(1024) DEFAULT '' NULL,
  [run_status] varchar(50) DEFAULT 0 NULL,
  PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)";


        /// <summary>
        /// 初始化操作
        /// </summary>
        /// <param name="db_type">数据库类型</param>
        /// <param name="connection_string">数据库连接字符串</param>
        public static void Init(string db_type, string connection_string)
        {
            DbConnection conn;

            switch (db_type)
            {
                case "MsSql":
                    conn = new SqlConnection(connection_string);
                    break;
                default:
                    conn = new MySqlConnection(connection_string);
                    break;
            }


            var db = new Database(conn);

            try
            {
                switch (db_type)
                {
                    case "MsSql":
                        db.Execute(sqlserver_single_user_analysis_history_info);
                        break;
                    default:
                        db.Execute(mysql_single_user_analysis_history_info);
                        break;
                }
            }
            catch { }

        }


    }
}

using log4net;
using MySql.Data.MySqlClient;
using PetaPoco.NetCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Timer.Web.Core.TableEntity;

namespace Timer.Web.Core.Utils
{

    public enum RunStatus
    {
        未执行 = 0,
        执行中 = 1,
        已执行 = 2
    }

    public class LogService
    {
        private Database _db;
        protected log4net.ILog Logger { get; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="db_type">数据库类型</param>
        /// <param name="connection_string">数据库连接字符串</param>
        public LogService(string db_type, string connection_string)
        {
            Logger = LogManager.GetLogger(AppUtil.LoggerRepository.Name, this.GetType());

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


            _db = new Database(conn);
        }


        /// <summary>
        /// 插入记录
        /// </summary>
        /// <param name="singleUserAnalysisHistoryInfo"></param>
        public SingleUserAnalysisHistoryInfo Insert(SingleUserAnalysisHistoryInfo data)
        {
            try
            {
                _db.Insert(data);
                return _db.FirstOrDefault<SingleUserAnalysisHistoryInfo>("select * from single_user_analysis_history_info where task_detail_id=@0 and step_no=@1 order by id desc", data.task_detail_id, data.step_no);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return data;
            }
        }


        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="data"></param>
        public void Update(SingleUserAnalysisHistoryInfo data)
        {
            try
            {
                _db.Update(data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }




    }
}

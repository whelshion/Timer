using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using PetaPoco.NetCore;
using System.Data.Common;
using System.Data.SqlClient;
using Timer.Web.Core.TableEntity;
using log4net;

namespace Timer.Web.Core.Utils
{
    public enum ExecStatus
    {
        未开始=0,
        运算中=1,
        已完成=2
    }
    public class TopService
    {
        private Database _db;
        protected log4net.ILog Logger { get; }

        public TopService(string db_type, string connection_string)
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
        public sig_ue_mr_top200_job Query(string sql)
        {
            try
            {
                return _db.FirstOrDefault<sig_ue_mr_top200_job>(sql);
            }
            catch(Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="data"></param>
        public void Update(sig_ue_mr_top200_job data)
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

using PetaPoco.NetCore;
using System;

namespace Timer.Web.Core.TableEntity
{

    [TableName("single_user_analysis_history_info")]
    [PrimaryKey("id")]
    public class SingleUserAnalysisHistoryInfo
    {

        /// <summary>
        /// id主键  自增列
        /// </summary>
        public int id { get; set; }


        /// <summary>
        /// detail表的id
        /// </summary>
        public long task_detail_id { get; set; }


        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime col_create_time { get; set; }


        /// <summary>
        /// 耗时 （秒）
        /// </summary>
        public int spend_time { get; set; }

        /// <summary>
        /// 步骤编号   由1开始  递增
        /// </summary>
        public int step_no { get; set; }


        /// <summary>
        /// 步骤名称
        /// </summary>
        public string step_name { get; set; }


        /// <summary>
        /// 步骤备注
        /// </summary>
        public string step_remark { get; set; }

        /// <summary>
        /// 运行状态   未运行， 运行中，  已处理
        /// </summary>
        public string run_status { get; set; }



        /// <summary>
        /// 异常信息
        /// </summary>
        public string error_msg { get; set; }


    }



}

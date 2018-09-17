using PetaPoco.NetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Web.Core.TableEntity
{
    [TableName("single_user_analysis_history_info")]
    [PrimaryKey("task_id")]
    public class sig_ue_mr_top200_job
    {
        //主键
        public int task_id { get; set; }
        //任务名字
        public string task_name { get; set; }
        //任务创建时间
        public string createTime { get; set; }
        //状态
        public string state { get; set; }
        //完成时间
        public string completeTime { get; set; }
        //上传文件名字
        public string filename { get; set; }
    }
}

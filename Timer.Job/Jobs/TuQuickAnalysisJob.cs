using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Timer.Job.Utils;

namespace Timer.Job.Jobs
{
    [Description("TOP用户快速分析任务")]
    public class TuQuickAnalysisJob : BaseJob
    {

        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            Logger.Debug("TuQuickAnalysis任务执行开始执行");
            return TaskUtil.CompletedTask;
        }
    }
}

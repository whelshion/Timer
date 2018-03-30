using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer.WinService.Net4.Utils;

namespace Timer.WinService.Net4.Jobs
{
    public abstract class BaseJob : IJob
    {
        public BaseJob()
        {
            Logger = LogManager.GetLogger(AppUtil.LoggerRepository.Name, this.GetType());
        }

        protected ILog Logger { get; }
        protected abstract void ExecuteJob(IJobExecutionContext context);

        void IJob.Execute(IJobExecutionContext context)
        {
            Logger.Info($"******************************新周期触发(线程ID:{Thread.CurrentThread.ManagedThreadId})******************************");

            Logger.Debug($"----------------触发任务:[{context.JobDetail.Key.Name},{context.JobDetail.Key.Group},{context.JobDetail.Description}],下次触发时刻:{context.NextFireTimeUtc.GetValueOrDefault().ToLocalTime()}----------------");
            var dm = context.JobDetail.JobDataMap.Select(o => $"[{o.Key},{o.Value}]");
            Logger.DebugFormat("任务配置:{{{0}}}", string.Join(",", dm));

            ExecuteJob(context);

            Logger.Info($"******************************周期结束(线程ID:{Thread.CurrentThread.ManagedThreadId})******************************");
        }
    }
}

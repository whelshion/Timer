using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Timer.Web.Core
{
    public abstract class BaseJob : IJob
    {
        public BaseJob()
        {
            //Logger = LogManager.GetLogger(Utils.JdllUtil.LoggerRepository.Name, this.GetType());
            Logger = LogManager.GetLogger(AppUtil.LoggerRepository.Name, this.GetType());
        }
        protected log4net.ILog Logger { get; }
        public virtual Task Execute(IJobExecutionContext context)
        {
            Logger.Info($"******************************新周期触发(线程ID:{Thread.CurrentThread.ManagedThreadId})******************************");

            Logger.Debug($"----------------触发任务:[{context.JobDetail.Key.Name},{context.JobDetail.Key.Group},{context.JobDetail.Description}],下次触发时刻:{context.NextFireTimeUtc.GetValueOrDefault().ToLocalTime()}----------------");
            var dm = context.JobDetail.JobDataMap.Select(o => $"[{o.Key},{o.Value}]");
            Logger.DebugFormat("任务配置:{{{0}}}", string.Join(",", dm));

            var task = ExecuteJob(context);

            Logger.Info($"******************************周期结束(线程ID:{Thread.CurrentThread.ManagedThreadId})******************************");

            return task;
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);
    }
}

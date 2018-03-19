using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Job
{
    public abstract class BaseJob : IJob
    {
        public BaseJob()
        {
            Logger = LogManager.GetLogger(Utils.JdllUtil.LoggerRepository.Name, this.GetType());
        }
        internal log4net.ILog Logger { get; }
        public Task Execute(IJobExecutionContext context)
        {
            Logger.Debug($"----------------触发任务:[{context.JobDetail.Key.Name},{context.JobDetail.Key.Group},{context.JobDetail.Description}],下次触发时刻:{context.NextFireTimeUtc.GetValueOrDefault().ToLocalTime()}----------------");
            var dm = context.JobDetail.JobDataMap.Select(o => $"{{{o.Key}:{o.Value}}}");
            Logger.DebugFormat("任务配置:[{0}]", string.Join(",", dm));
            return ExecuteJob(context);
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);
    }
}

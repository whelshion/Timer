using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timer.MvcCoro.Utils;

namespace Timer.MvcCore.Jobs
{
    public abstract class BaseJob : IJob
    {
        public BaseJob(Type jobType)
        {
            Logger = LogManager.GetLogger(AppSetting.LoggerRepository.Name, jobType);
        }
        protected ILog Logger { get; private set; }
        public Task Execute(IJobExecutionContext context)
        {
            return ExecuteJob(context);
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);
    }
}

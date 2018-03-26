using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Jobs
{
    public class HttpGetJob : BaseJob
    {
        public string Url { private get; set; }
        public string Query { private get; set; }

        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.MergedJobDataMap;
                Url = dataMap.GetString("url");
                Query = dataMap.GetString("query");
                var result = HttpUtil.HttpGetAsync(Url + Query).Result;
                Logger.Info($"[接口]-- {Url + Query}");
                Logger.Info($"[结果]-- {Environment.NewLine}{result}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return TaskUtil.CompletedTask;
        }
    }
}

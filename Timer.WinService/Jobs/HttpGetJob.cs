using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timer.WinService.Utils;

namespace Timer.WinService.Jobs
{
    public class HttpGetJob : IJob
    {
        private readonly ILog log = LogManager.GetLogger(typeof(HttpGetJob));
        public string Url { get; private set; }
        public string Query { get; private set; }
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.MergedJobDataMap;
                Url = dataMap.GetString("url");
                Query = dataMap.GetString("query");
                var result = HttpUtil.HttpGetAsync(Url + Query).Result;
                log.Info($"[接口]-- {Url + Query}");
                log.Info($"[结果]-- {Environment.NewLine}{result}");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return TaskUtil.CompletedTask;
        }
    }
}

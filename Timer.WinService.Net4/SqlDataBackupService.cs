using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Timer.WinService.Net4.Jobs;
using Timer.WinService.Net4.Utils;

namespace Timer.WinService.Net4
{
    public partial class SqlDataBackupService : ServiceBase
    {
        private readonly ILog logger;
        private IScheduler scheduler;
        public SqlDataBackupService()
        {
            InitializeComponent();
            logger = LogManager.GetLogger(AppUtil.LoggerRepository.Name, this.GetType());
            //新建一个调度器工工厂
            ISchedulerFactory factory = new StdSchedulerFactory();
            //使用工厂生成一个调度器
            scheduler = factory.GetScheduler();
        }

        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            logger.Info("工单明细表备份服务正在开启");

            if (!scheduler.IsStarted)
            {
                //启动调度器
                scheduler.Start();

                //新建一个任务
                IJobDetail job = JobBuilder.Create<SqlDataBackupJob>()
                    .WithIdentity("备份数据库表任务", "数据库")
                    .SetJobData(new JobDataMap
                    {
                        { "shell-name",ConfigurationManager.AppSettings["shell-name"]??(Environment.OSVersion.Platform==PlatformID.Win32NT?"cmd.exe":"/bin/bash")},
                        { "shell-command",ConfigurationManager.AppSettings["shell-command"]??@"bcp ""select * from rosas_hn.dbo.manager_task_detail with (nolock)"" queryout ""{fileName}"" -c -t ""|"" -S 10.154.14.62 -U sa -P pass@word1 -k"},
                        { "out-folder",ConfigurationManager.AppSettings["out-folder"]??(AppUtil.AppDirectory+"\\bckfiles")},
                        { "file-name",ConfigurationManager.AppSettings["file-name"]??"manager_task_detail"},
                        { "file-format",ConfigurationManager.AppSettings["file-format"]??".yyyyMMddHHmmss"},
                        { "file-ext",ConfigurationManager.AppSettings["file-ext"]??"csv"}
                    }).Build();

                string cron = ConfigurationManager.AppSettings["cron"] ?? "0 0 1 ? * 2";
                //string cron = ConfigurationManager.AppSettings["cron"] ?? "*/5 * * * * ?";

                //新建一个触发器
                ITrigger trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cron).Build();
                //将任务与触发器关联起来放到调度器中
                scheduler.ScheduleJob(job, trigger);
                logger.Info("工单明细表备份服务已启动");
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        protected override void OnStop()
        {
            if (!scheduler.IsShutdown)
            {
                scheduler.Shutdown();
            }
            logger.Info("工单明细表备份服务停止");
        }

        /// <summary>
        /// 暂停
        /// </summary>
        protected override void OnPause()
        {
            scheduler.PauseAll();
            logger.Info("工单明细表备份服务暂停");
            base.OnPause();
        }

        /// <summary>
        ///  继续
        /// </summary>
        protected override void OnContinue()
        {
            scheduler.ResumeAll();
            logger.Info("工单明细表备份服务继续");
            base.OnContinue();
        }
    }
}

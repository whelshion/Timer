using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Timer.WinService
{
    public partial class TimerService : ServiceBase
    {
        #region 私有属性

        //日志记录这。
        private readonly ILog _logger;

        //调度器。
        private readonly IScheduler _scheduler;

        #endregion

        #region 构造方法

        /// <summary>
        /// 初始化 <see cref="MainService"/> 类的新实例。
        /// </summary>
        public TimerService()
        {
            InitializeComponent();
            // Ref https://github.com/Wlitsoft/QuartzNETWinServiceSample
            this._logger = LogManager.GetLogger(this.GetType());
            RunProgram(_scheduler).GetAwaiter().GetResult();
        }

        private async Task RunProgram(IScheduler scheduler)
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                scheduler = await factory.GetScheduler();
                await scheduler.Start();
            }
            catch (SchedulerException se)
            {
                this._logger.Error(se.ToString());
            }
        }

        #endregion

        protected override void OnStart(string[] args)
        {
            this._scheduler.Start();
            this._logger.Info("服务启动");
        }

        protected override void OnStop()
        {
            if (!this._scheduler.IsShutdown)
                this._scheduler.Shutdown();
            this._logger.Info("服务停止");
        }

        protected override void OnPause()
        {
            this._scheduler.PauseAll();
            this._logger.Info("暂停服务");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            this._scheduler.ResumeAll();
            this._logger.Info("服务继续");
            base.OnContinue();
        }
    }
}

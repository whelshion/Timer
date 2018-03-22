﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timer.Job.Utils;
using Timer.Web.Core.Utils;
//using Timer.Web.Core.LiveLog;

namespace Timer.Web.Core
{
    public class WebConsolePlugin : ISchedulerPlugin
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(JdllUtil.LoggerRepository.Name, typeof(WebConsolePlugin));
        private IDisposable host;

        public string HostName { get; set; }
        public int? Port { get; set; }

        public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken)
        {
            // var liveLogPlugin = new LiveLogPlugin();
            // scheduler.ListenerManager.AddJobListener(liveLogPlugin);
            // scheduler.ListenerManager.AddTriggerListener(liveLogPlugin);
            // scheduler.ListenerManager.AddSchedulerListener(liveLogPlugin);

            // TODO REMOVE
            scheduler.AddCalendar(typeof (AnnualCalendar).Name, new AnnualCalendar(), false, false, cancellationToken);
            scheduler.AddCalendar(typeof (CronCalendar).Name, new CronCalendar("0 0/5 * * * ?"), false, false, cancellationToken);
            scheduler.AddCalendar(typeof (DailyCalendar).Name, new DailyCalendar("12:01", "13:04"), false, false, cancellationToken);
            scheduler.AddCalendar(typeof (HolidayCalendar).Name, new HolidayCalendar(), false, false, cancellationToken);
            scheduler.AddCalendar(typeof (MonthlyCalendar).Name, new MonthlyCalendar(), false, false, cancellationToken);
            scheduler.AddCalendar(typeof (WeeklyCalendar).Name, new WeeklyCalendar(), false, false, cancellationToken);

            return Task.CompletedTask;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            string baseAddress = $"http://{HostName ?? "localhost"}:{Port ?? 28682}/";

            //host = WebApp.Start<Startup>(url: baseAddress);
            host = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .Build();
            
            log.InfoFormat("Quartz Web Console bound to address {0}", baseAddress);
            return TaskUtil.CompletedTask;
        }

        public Task Shutdown(CancellationToken cancellationToken)
        {
            host?.Dispose();
            return TaskUtil.CompletedTask;
        }
    }
}
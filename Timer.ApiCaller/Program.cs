using log4net;
using log4net.Config;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Timer.ApiCaller.Jobs;

namespace Timer.ApiCaller
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "接口调用器(请勿关闭)";
            Console.WriteLine("欢迎使用自动接口调用器，按[Esc]键可退出程序");
            AppSetting.LoggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(AppSetting.LoggerRepository, new FileInfo("log4net.config"));
            RunProgram().GetAwaiter().GetResult();
            while (true)
            {
                var input = Console.ReadKey();
                if (input.Key.ToString().ToUpper() == "ESCAPE")
                    break;
            }
        }
        static async Task RunProgram()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                var properties = new NameValueCollection
                {
                    ["quartz.serializer.type"] = "binary",
                    ["quartz.scheduler.instanceName"] = "XmlConfiguredInstance",
                    ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                    ["quartz.threadPool.threadCount"] = "5",
                    ["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz.Plugins",
                    ["quartz.plugin.xml.fileNames"] = "~/quartz_jobs.config",
                    // this is the default
                    ["quartz.plugin.xml.FailOnFileNotFound"] = "true",
                    // this is not the default
                    ["quartz.plugin.xml.failOnSchedulingError"] = "true"
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(properties);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // some sleep to show what's happening
                //await Task.Delay(TimeSpan.FromSeconds(60));

                //// define the job and tie it to our HelloJob class
                //IJobDetail job = JobBuilder.Create<CallApiJob>()
                //    .WithIdentity("myJob", "group1")
                //    .Build();

                //// Trigger the job to run now, and then every 40 seconds
                //ITrigger trigger = TriggerBuilder.Create()
                //  .WithIdentity("myTrigger", "group1")
                //  .StartNow()
                //  .WithCronSchedule("/3 * * * * ?")
                //  .Build();

                //await scheduler.ScheduleJob(job, trigger);

                // and last shut down the scheduler when you are ready to close your program
                // await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using PetaPoco.NetCore;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Timer.Web.Core.LiveLog;

namespace Timer.Web.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc();
            services.AddSingleton(Configuration);

            var dbType = Configuration.GetValue<string>("DbType");
            var connectionStr = Configuration.GetValue<string>("ConnectionString");

            switch (dbType)
            {
                case "MsSql":
                    services.AddSingleton(new Database(new SqlConnection(connectionStr)));
                    break;
                default:
                    services.AddSingleton(new Database(new MySqlConnection(connectionStr)));
                    break;
            }



            //services.AddSignalR();
            ConfigureQuartz(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            AppUtil.LoggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(AppUtil.LoggerRepository, new System.IO.FileInfo("log4net.config"));
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<LiveLogHub>("/live");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        void ConfigureQuartz(IServiceCollection services)
        {
            var properties = new System.Collections.Specialized.NameValueCollection
            {
                ["quartz.serializer.type"] = "binary",
                ["quartz.scheduler.instanceName"] = "XmlConfiguredInstance",
                ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                ["quartz.threadPool.threadCount"] = "10",
                ["quartz.plugin.xml.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz.Plugins",
                ["quartz.plugin.xml.fileNames"] = "~/quartz_jobs.config",
                // this is the default
                ["quartz.plugin.xml.FailOnFileNotFound"] = "true",
                // this is not the default
                ["quartz.plugin.xml.failOnSchedulingError"] = "true"
            };

            // First we must get a reference to a scheduler
            ISchedulerFactory sf = new StdSchedulerFactory(properties);
            IScheduler scheduler = sf.GetScheduler().Result;

            //var liveLogPlugin = new LiveLogPlugin();
            //scheduler.ListenerManager.AddJobListener(liveLogPlugin);
            //scheduler.ListenerManager.AddTriggerListener(liveLogPlugin);
            //scheduler.ListenerManager.AddSchedulerListener(liveLogPlugin);

            scheduler.AddCalendar(typeof(AnnualCalendar).Name, new AnnualCalendar(), false, false);
            scheduler.AddCalendar(typeof(CronCalendar).Name, new CronCalendar("0 0/5 * * * ?"), false, false);
            scheduler.AddCalendar(typeof(DailyCalendar).Name, new DailyCalendar("12:01", "13:04"), false, false);
            scheduler.AddCalendar(typeof(HolidayCalendar).Name, new HolidayCalendar(), false, false);
            scheduler.AddCalendar(typeof(MonthlyCalendar).Name, new MonthlyCalendar(), false, false);
            //scheduler.AddCalendar(typeof (WeeklyCalendar).Name, new WeeklyCalendar(), false, false);

            services.AddSingleton(scheduler);
        }
    }
}

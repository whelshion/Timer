using Quartz;
using Quartz.Impl;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Timer.Web.Core.Models;

namespace Timer.Web.Core.Utils
{
    public class TimerUtil
    {
        public static async Task<IScheduler> GetScheduler(string schedulerName)
        {
            var scheduler = await SchedulerRepository.Instance.Lookup(schedulerName).ConfigureAwait(false);
            if (scheduler == null)
            {
                //throw new HttpResponseException(HttpStatusCode.NotFound);
                throw new KeyNotFoundException($"Scheduler {schedulerName} not found!");
            }
            return scheduler;
        }

        public static async Task<IReadOnlyList<IScheduler>> GetSchedulers()
        {
            return await SchedulerRepository.Instance.LookupAll().ConfigureAwait(false);
        }

        public static async Task<IEnumerable<JobTypeVM>> GetJobTypes()
        {
            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(o => typeof(IJob).IsAssignableFrom(o) && !o.IsAbstract && o.IsClass && o.IsPublic)
                .Select(o => new JobTypeVM
                {
                    FullName = o.FullName,
                    Name = o.Name,
                    AssemblyQualifiedName = o.AssemblyQualifiedName,
                    AssemblyQualifiedNameWithoutVersion = o.GetType().AssemblyQualifiedNameWithoutVersion(),
                    Namespace = o.Namespace,
                    Description = (o.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true) as IEnumerable<System.ComponentModel.DescriptionAttribute>)?.LastOrDefault()?.Description
                });
            return types;
        }

        public static async Task<Type> GetJobType(string typeFullName)
        {
            return typeof(BaseJob).Assembly.GetTypes()
                 .FirstOrDefault(o => typeof(IJob).IsAssignableFrom(o) && !o.IsAbstract && o.IsClass && o.IsPublic && o.FullName == typeFullName);
        }
    }
}

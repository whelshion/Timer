using Quartz;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Web.Core.Models
{
    public class SchedulerVM
    {
        public SchedulerVM() { }
        public SchedulerVM(IScheduler scheduler)
        {
            SchedulerName = scheduler.SchedulerName;
            SchedulerInstanceId = scheduler.SchedulerInstanceId;
            Status = SchedulerHeaderVM.TranslateStatus(scheduler);
        }
        public SchedulerVM(IScheduler scheduler, SchedulerMetaData metaData) : this(scheduler)
        {
            ThreadPool = new SchedulerThreadPoolVM(metaData);
            JobStore = new SchedulerJobStoreVM(metaData);
            Statistics = new SchedulerStatisticsVM(metaData);
        }

        [Display(Name = "UK")]
        public string SchedulerInstanceId { get; set; }
        [Display(Name = "名称")]
        public string SchedulerName { get; set; }
        [Display(Name = "状态")]
        public SchedulerStatus Status { get; set; }


        public SchedulerThreadPoolVM ThreadPool { get; set; }
        public SchedulerJobStoreVM JobStore { get; set; }
        public SchedulerStatisticsVM Statistics { get; set; }
    }

    public class SchedulerJobStoreVM
    {
        public SchedulerJobStoreVM(SchedulerMetaData metaData)
        {
            Type = metaData.JobStoreType.AssemblyQualifiedNameWithoutVersion();
            Clustered = metaData.JobStoreClustered;
            Persistent = metaData.JobStoreSupportsPersistence;
        }

        public string Type { get; set; }
        public bool Clustered { get; set; }
        public bool Persistent { get; set; }
    }

    public class SchedulerHeaderVM
    {
        public SchedulerHeaderVM(IScheduler scheduler)
        {
            Name = scheduler.SchedulerName;
            SchedulerInstanceId = scheduler.SchedulerInstanceId;
            Status = TranslateStatus(scheduler);
        }

        public string Name { get; set; }
        public string SchedulerInstanceId { get; set; }
        public SchedulerStatus Status { get; set; }

        internal static SchedulerStatus TranslateStatus(IScheduler scheduler)
        {
            if (scheduler.IsShutdown)
            {
                return SchedulerStatus.Shutdown;
            }
            if (scheduler.InStandbyMode)
            {
                return SchedulerStatus.Standby;
            }
            if (scheduler.IsStarted)
            {
                return SchedulerStatus.Running;
            }
            return SchedulerStatus.Unknown;
        }
    }

    public class SchedulerStatisticsVM
    {
        public SchedulerStatisticsVM(SchedulerMetaData metaData)
        {
            NumberOfJobsExecuted = metaData.NumberOfJobsExecuted;
        }

        public int NumberOfJobsExecuted { get; set; }
    }

    public class SchedulerThreadPoolVM
    {
        public SchedulerThreadPoolVM(SchedulerMetaData metaData)
        {
            Type = metaData.ThreadPoolType.AssemblyQualifiedNameWithoutVersion();
            Size = metaData.ThreadPoolSize;
        }

        public string Type { get; set; }
        public int Size { get; set; }
    }

    public enum SchedulerStatus
    {
        [Description("未知")] Unknown = 0,
        [Description("启动")] Running = 1,
        [Description("备用")] Standby = 2,
        [Description("关闭")] Shutdown = 3
    }
}

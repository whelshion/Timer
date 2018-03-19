using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Web.Core.Models
{
    public class JobDetailVM
    {
        public JobDetailVM() { }
        public JobDetailVM(IScheduler scheduler, IJobDetail jobDetail)
        {
            SchedulerName = scheduler.SchedulerName;
            JobName = jobDetail.Key.Name;
            JobGroup = jobDetail.Key.Group;
            Description = jobDetail.Description;
            Durable = jobDetail.Durable;
            JobType = jobDetail.JobType.FullName;
            RequestsRecovery = jobDetail.RequestsRecovery;
            PersistJobDataAfterExecution = jobDetail.PersistJobDataAfterExecution;
            ConcurrentExecutionDisallowed = jobDetail.ConcurrentExecutionDisallowed;
            JobDataMap = jobDetail.JobDataMap.ToDictionary(d => d.Key, d => d.Value);
        }
        public JobDetailVM(IJobExecutionContext context) : this(context.Scheduler, context.JobDetail)
        {
            NextFireTimeUtc = context.NextFireTimeUtc;
        }

        [Required(ErrorMessage = "调度器名称是必填项")]
        [Display(Name = "调度器名称")]
        public string SchedulerName { get; set; }
        [Required(ErrorMessage = "组名称是必填项")]
        [Display(Name = "组名称")]
        public string JobGroup { get; set; }
        [Required(ErrorMessage = "任务名称是必填项")]
        [Display(Name = "任务名称")]
        public string JobName { get; set; }
        [Required(ErrorMessage = "任务类型是必填项")]
        [Display(Name = "任务类型")]
        public string JobType { get; set; }
        [Required(ErrorMessage = "任务说明是必填项")]
        [Display(Name = "任务说明")]
        public string Description { get; set; }
        [Display(Name = "保留任务")]
        public bool Durable { get; set; }
        [Display(Name = "故障恢复")]
        public bool RequestsRecovery { get; set; }
        [Display(Name = "保留作业数据")]
        public bool PersistJobDataAfterExecution { get; set; }
        [Display(Name = "禁用并行执行")]
        public bool ConcurrentExecutionDisallowed { get; set; }
        [Display(Name = "下次触发时刻")]
        public DateTimeOffset? NextFireTimeUtc { get; set; }
        [Display(Name = "数据配置")]
        public IDictionary<string, object> JobDataMap { get; set; }
    }

    public class JobTypeVM
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public string AssemblyQualifiedName { get; set; }
        public string AssemblyQualifiedNameWithoutVersion { get; set; }
        public string Namespace { get; set; }
        public string Description { get; set; }
    }
}

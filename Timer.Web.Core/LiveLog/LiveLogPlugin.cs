using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using Quartz.Util;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.LiveLog
{
    public class LiveLogPlugin : ITriggerListener, IJobListener, ISchedulerListener
    {
        public LiveLogPlugin()
        {
            Name = "活动日志";
        }

        public string Name { get; }

        public Task JobToBeExecuted(IJobExecutionContext context)
        {
            return SendToClients(x => x.jobToBeExecuted(new KeyVM(context.JobDetail.Key), new KeyVM(context.Trigger.Key)));
        }

        public Task JobExecutionVetoed(IJobExecutionContext context)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            return SendToClients(x => x.jobWasExecuted(new KeyVM(context.JobDetail.Key), new KeyVM(context.Trigger.Key), jobException?.Message));
        }

        public Task TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            return SendToClients(x => x.triggerFired(new KeyVM(trigger.Key)));
        }

        public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            return Task.FromResult(false);
        }

        public Task TriggerMisfired(ITrigger trigger)
        {
            return SendToClients(x => x.triggerMisfired(new KeyVM(trigger.Key)));
        }

        public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            return SendToClients(x => x.triggerComplete(new KeyVM(trigger.Key)));
        }

        public Task JobScheduled(ITrigger trigger)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobUnscheduled(TriggerKey triggerKey)
        {
            return TaskUtil.CompletedTask;
        }

        public Task TriggerFinalized(ITrigger trigger)
        {
            return TaskUtil.CompletedTask;
        }

        public Task TriggerPaused(TriggerKey triggerKey)
        {
            return SendToClients(x => x.triggerPaused(new KeyVM(triggerKey)));
        }

        public Task TriggersPaused(string triggerGroup)
        {
            return TaskUtil.CompletedTask;
        }

        public Task TriggerResumed(TriggerKey triggerKey)
        {
            return SendToClients(x => x.triggerResumed(new KeyVM(triggerKey)));
        }

        public Task TriggersResumed(string triggerGroup)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobAdded(IJobDetail jobDetail)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobDeleted(JobKey jobKey)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobPaused(JobKey jobKey)
        {
            return SendToClients(x => x.jobPaused(jobKey));
        }

        public Task JobsPaused(string jobGroup)
        {
            return TaskUtil.CompletedTask;
        }

        public Task JobResumed(JobKey jobKey)
        {
            return SendToClients(x => x.jobResumed(jobKey));
        }

        public Task JobsResumed(string jobGroup)
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerError(string msg, SchedulerException cause)
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerInStandbyMode()
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerStarted()
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerStarting()
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerShutdown()
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulerShuttingdown()
        {
            return TaskUtil.CompletedTask;
        }

        public Task SchedulingDataCleared()
        {
            return TaskUtil.CompletedTask;
        }

        private Task SendToClients(Action<dynamic> action)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<LiveLogHub>();
            //action(context.Clients.All);
            return TaskUtil.CompletedTask;
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {

            return TaskUtil.CompletedTask;
        }

        public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(true);
        }

        public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default(CancellationToken))
        {
            return TaskUtil.CompletedTask;
        }

        public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerStarted(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerStarting(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerShutdown(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulerShuttingdown(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }

        public Task SchedulingDataCleared(CancellationToken cancellationToken = default(CancellationToken))
        {
                        return TaskUtil.CompletedTask;
        }
    }
}

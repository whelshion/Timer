using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers
{
    public class JobController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var schedulers = await TimerUtil.GetSchedulers().ConfigureAwait(false);
            var matcher = new GroupMatcherVM().GetJobGroupMatcher();
            IDictionary<SchedulerVM, IEnumerable<JobDetailVM>> jobs = new Dictionary<SchedulerVM, IEnumerable<JobDetailVM>>();
            foreach (var item in schedulers)
            {
                var jobKeys = await item.GetJobKeys(matcher).ConfigureAwait(false);
                var jobDetails = jobKeys.Select(async o => await item.GetJobDetail(o).ConfigureAwait(false))
                    .Select(o => new JobDetailVM(item, o.Result)).OrderBy(o => o.NextFireTimeUtc).ThenBy(o => o.JobGroup);
                jobs.Add(new SchedulerVM(item), jobDetails);
            }
            return View(jobs);
        }

        public async Task<IActionResult> Edit(string schedulerName, string jobGroup, string jobName)
        {
            ViewData["job_type"] = await TimerUtil.GetJobTypes();
            JobDetailVM jobDetail = null;
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(jobGroup) && !string.IsNullOrWhiteSpace(jobName))
            {
                jobDetail = new JobDetailVM(scheduler, await scheduler.GetJobDetail(new JobKey(jobName, jobGroup)).ConfigureAwait(false));
            }
            return View(jobDetail ?? new JobDetailVM { SchedulerName = schedulerName });
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(JobDetailVM jobDetail)
        {
            if (!ModelState.IsValid)
            {
                ViewData["job_type"] = await TimerUtil.GetJobTypes();
                return View(nameof(Edit), jobDetail);
            }
            var scheduler = await TimerUtil.GetScheduler(jobDetail.SchedulerName).ConfigureAwait(false);
            var jobDetailImpl = new JobDetailImpl(jobDetail.JobName, jobDetail.JobGroup, await TimerUtil.GetJobType(jobDetail.JobType), true, jobDetail.RequestsRecovery);
            jobDetailImpl.Description = jobDetail.Description;
            await scheduler.AddJob(jobDetailImpl, jobDetail.ConcurrentExecutionDisallowed).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(string schedulerName, string jobGroup, string jobName)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var jobDetail = await scheduler.GetJobDetail(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return View(new JobDetailVM(scheduler, jobDetail));
        }

        public async Task<IActionResult> PauseAsync(string schedulerName, string jobGroup, string jobName, string redirectAction = "Detail")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.PauseJob(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, jobGroup, jobName });
        }

        [HttpPost]
        public async Task<IActionResult> PauseGroupAsync(string schedulerName, GroupMatcherVM groupMatcher)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var matcher = (groupMatcher ?? new GroupMatcherVM()).GetJobGroupMatcher();
            await scheduler.PauseJobs(matcher).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResumeAsync(string schedulerName, string jobGroup, string jobName, string redirectAction = "Detail")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.ResumeJob(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, jobGroup, jobName });
        }

        [HttpPost]
        public async Task<IActionResult> ResumeGroupAsync(string schedulerName, GroupMatcherVM groupMatcher)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var matcher = (groupMatcher ?? new GroupMatcherVM()).GetJobGroupMatcher();
            await scheduler.ResumeJobs(matcher).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> TriggerAsync(string schedulerName, string jobGroup, string jobName, string redirectAction = "Detail")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.TriggerJob(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, jobGroup, jobName });
        }

        public async Task<IActionResult> DeleteAsync(string schedulerName, string jobGroup, string jobName, string redirectAction = "Index")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.DeleteJob(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, jobGroup, jobName });
        }

        public async Task<IActionResult> InterruptAsync(string schedulerName, string jobGroup, string jobName, string redirectAction = "Detail")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.Interrupt(new JobKey(jobName, jobGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, jobGroup, jobName });
        }
    }
}
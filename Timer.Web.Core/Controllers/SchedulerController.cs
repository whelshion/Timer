using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;
using Timer.Web.Core.Controllers.Dto;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers
{
    public class SchedulerController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var schedulers = (await SchedulerRepository.Instance.LookupAll().ConfigureAwait(false))
                .Select(o => new SchedulerVM(o));
            return View(schedulers);
        }

        public async Task<IActionResult> StartAsync(string schedulerName, int? delaySeconds = null)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            if (delaySeconds == null)
            {
                await scheduler.Start().ConfigureAwait(false);
            }
            else
            {
                await scheduler.StartDelayed(TimeSpan.FromSeconds(delaySeconds.Value)).ConfigureAwait(false);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StandbyAsync(string schedulerName)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.Standby().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ShutdownAsync(string schedulerName, bool waitComplete = false)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.Shutdown(waitComplete).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ClearAsync(string schedulerName)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.Clear().ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(string schedulerName)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var metaData = await scheduler.GetMetaData().ConfigureAwait(false);
            return View(new SchedulerVM(scheduler, metaData));
        }
    }
}
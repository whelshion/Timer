using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers
{
    public class TriggerController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var schedulers = await TimerUtil.GetSchedulers().ConfigureAwait(false);
            var matcher = new GroupMatcherVM().GetTriggerGroupMatcher();
            IDictionary<SchedulerVM, IEnumerable<TriggerDetailVM>> triggers = new Dictionary<SchedulerVM, IEnumerable<TriggerDetailVM>>();
            foreach (var item in schedulers)
            {
                var trgKeys = await item.GetTriggerKeys(matcher).ConfigureAwait(false);
                var trgDetailTasks = trgKeys.Select(async o => await item.GetTrigger(o).ConfigureAwait(false))
                    .Select(o => o.Result)
                    .Select(async o => TriggerDetailVM.Create(o, o.CalendarName != null ? await item.GetCalendar(o.CalendarName).ConfigureAwait(false) : null));
                var trgVMs = (await Task.WhenAll(trgDetailTasks))
                    .Select(o => { o.SchedulerName = item.SchedulerName; return o; })
                    .OrderBy(o => o.NextFireTimes.FirstOrDefault())
                    .ThenBy(o => o.Group)
                    .ThenBy(o => o.Priority);
                triggers.Add(new SchedulerVM(item), trgVMs);
            }
            return View(triggers);
        }

        public async Task<IActionResult> Detail(string schedulerName, string triggerGroup, string triggerName)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var trgDetail = await scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup)).ConfigureAwait(false);
            var calendar = trgDetail.CalendarName != null
                ? await scheduler.GetCalendar(trgDetail.CalendarName).ConfigureAwait(false)
                : null;
            var trigger = TriggerDetailVM.Create(trgDetail, calendar);
            trigger.SchedulerName = scheduler.SchedulerName;
            return View(trigger);
        }

        public async Task<IActionResult> PauseAsync(string schedulerName, string triggerGroup, string triggerName, string redirectAction = "Index")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.PauseTrigger(new TriggerKey(triggerName, triggerGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, triggerGroup, triggerName });
        }

        public async Task<IActionResult> PauseGroupAsync(string schedulerName, GroupMatcherVM groupMatcher)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var matcher = (groupMatcher ?? new GroupMatcherVM()).GetTriggerGroupMatcher();
            await scheduler.PauseTriggers(matcher).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ResumeAsync(string schedulerName, string triggerGroup, string triggerName, string redirectAction = "Index")
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            await scheduler.ResumeTrigger(new TriggerKey(triggerName, triggerGroup)).ConfigureAwait(false);
            return RedirectToAction(redirectAction, new { schedulerName, triggerGroup, triggerName });
        }

        public async Task<IActionResult> ResumeGroupAsync(string schedulerName, GroupMatcherVM groupMatcher)
        {
            var scheduler = await TimerUtil.GetScheduler(schedulerName).ConfigureAwait(false);
            var matcher = (groupMatcher ?? new GroupMatcherVM()).GetTriggerGroupMatcher();
            await scheduler.ResumeTriggers(matcher).ConfigureAwait(false);
            return RedirectToAction(nameof(Index));
        }
    }
}
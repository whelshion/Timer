using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers
{
    public class HomeController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var schedulers = await TimerUtil.GetSchedulers().ConfigureAwait(false);
            List<JobDetailVM> jobs = new List<JobDetailVM>();
            foreach (var item in schedulers)
            {
                var partial = await item.GetCurrentlyExecutingJobs().ConfigureAwait(false);
                jobs.AddRange(partial.Select(o => new JobDetailVM(o)));
            }
            return View(jobs.ToLookup(o => o.SchedulerName));
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

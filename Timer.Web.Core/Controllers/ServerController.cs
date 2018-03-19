using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Quartz.Impl;
using Timer.Web.Core.Controllers.Dto;
using Timer.Web.Core.Models;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers
{
    /// <summary>
    /// Web API endpoint for scheduler information.
    /// </summary>
    public class ServerController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var servers = ServerRepository.LookupAll();
            return View(servers.Select(x => new ServerHeaderVM(x)));
        }

        public async Task<IActionResult> Detail(string serverName)
        {
            var schedulers = await SchedulerRepository.Instance.LookupAll().ConfigureAwait(false);
            return View(new ServerDetailVM(schedulers));
        }
    }
}
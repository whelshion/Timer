using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.Web.Core.Controllers
{
    public abstract class BaseController : Controller
    {
        public BaseController()
        {
            Logger = log4net.LogManager.GetLogger(Job.Utils.JdllUtil.LoggerRepository.Name, this.GetType());
        }
        internal log4net.ILog Logger { get; }
    }
}

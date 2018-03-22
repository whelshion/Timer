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
            //Logger = LogProvider.GetLogger(this.GetType());
            Logger = log4net.LogManager.GetLogger(AppUtil.LoggerRepository.Name, this.GetType());
        }
        internal log4net.ILog Logger { get; }
    }
}

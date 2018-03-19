using log4net.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timer.MvcCoro.Utils
{
    public class AppSetting
    {
        public static ILoggerRepository LoggerRepository { get; set; }
        public static IConfiguration Configuration { get; set; }
    }
}

using log4net.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timer.ShellExecuter
{
    public class AppSetting
    {
        public static ILoggerRepository LoggerRepository { get; set; }
        public static IConfigurationRoot Configuration { get; set; }
    }
}

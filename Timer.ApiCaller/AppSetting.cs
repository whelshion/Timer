using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timer.ApiCaller
{
    public class AppSetting
    {
        public static ILoggerRepository LoggerRepository { get; set; }
    }
}

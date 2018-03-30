
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timer.WinService.Net4.Utils
{
    public class AppUtil
    {
        public static ILoggerRepository LoggerRepository { get; set; }
        public static string AppDirectory { get; set; }
    }
}

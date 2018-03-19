using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timer.Job.Utils
{
    public class JdllUtil
    {
        public static void Init()
        {
            LoggerRepository = log4net.LogManager.CreateRepository(System.Reflection.Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
        }
        public static void Init(string logConfigPath)
        {
            if (LoggerRepository == null)
                Init();
            log4net.Config.XmlConfigurator.Configure(LoggerRepository, new System.IO.FileInfo(logConfigPath));
        }
        public static ILoggerRepository LoggerRepository { get; set; }
    }
}

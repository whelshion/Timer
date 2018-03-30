using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using Timer.WinService.Net4.Utils;

namespace Timer.WinService.Net4
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            AppUtil.LoggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            AppUtil.AppDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            XmlConfigurator.Configure(AppUtil.LoggerRepository, new FileInfo(Path.Combine(AppUtil.AppDirectory, "log4net.config")));

            if (true || (args != null && args.Length > 0 && args[0] == "s"))
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new SqlDataBackupService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.Title = "工单明细表备份服务向导";
                Console.WriteLine("这是Windows应用程序");
                Console.WriteLine("请选择，[1]安装服务 [2]卸载服务 [3]退出");
                var rs = int.Parse(Console.ReadLine());
                string strServiceName = "工单明细表备份服务";
                switch (rs)
                {
                    case 1:
                        //取当前可执行文件路径，加上"s"参数，证明是从windows服务启动该程序
                        var path = Process.GetCurrentProcess().MainModule.FileName + " s";
                        Process.Start("sc", "create " + strServiceName + " binpath= \"" + path + "\" displayName= " + strServiceName + " start= auto");
                        Console.WriteLine("安装成功");
                        Console.Read();
                        break;
                    case 2:
                        Process.Start("sc", "delete " + strServiceName + "");
                        Console.WriteLine("卸载成功");
                        Console.Read();
                        break;
                    case 3: break;
                }
            }
        }
    }
}

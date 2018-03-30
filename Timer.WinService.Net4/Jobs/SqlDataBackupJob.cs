using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Timer.WinService.Net4.Utils;

namespace Timer.WinService.Net4.Jobs
{
    public class SqlDataBackupJob : BaseJob
    {
        public string ShellCommand { get; private set; }
        public string OutFolder { get; private set; }
        public string FileName { get; private set; }
        public string FileFormat { get; private set; }
        public string FileExt { get; private set; }
        public string ShellName { get; private set; }

        protected override void ExecuteJob(IJobExecutionContext context)
        {
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;
            string command;
            try
            {
                var dataMap = context.MergedJobDataMap;
                ShellCommand = dataMap.GetString("shell-command");
                ShellName = dataMap.GetString("shell-name");
                OutFolder = dataMap.GetString("out-folder");
                FileName = dataMap.GetString("file-name");
                FileFormat = dataMap.GetString("file-format");
                FileExt = dataMap.GetString("file-ext");
                string fileName = Path.Combine(OutFolder, string.Concat(FileName, DateTime.Now.ToString(FileFormat), ".", FileExt));
                command = ShellCommand.Replace("{fileName}", fileName);
                Logger.Info($"[命令]-- {command}");
                if (!Directory.Exists(OutFolder))
                    Directory.CreateDirectory(OutFolder);
                var result = ShellUtil.ExecuteCommand(ShellName, p =>
                 {
                     p(command);
                     p("exit 0");
                 }, null);
                string message = result ? fileName : "失败";
                Logger.Info($@"[结果]-- {message}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}

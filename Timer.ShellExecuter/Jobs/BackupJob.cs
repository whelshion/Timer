using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Timer.ShellExecuter.Utils;

namespace Timer.ShellExecuter.Jobs
{
    public class BackupJob : IJob
    {
        private readonly ILog log = LogManager.GetLogger(AppSetting.LoggerRepository.Name, typeof(BackupJob));
        public string BatPath { get; private set; }
        public string OutFolder { get; private set; }
        public string FileName { get; private set; }
        public string FileFormat { get; private set; }
        public string FileExt { get; private set; }
        public string ShellName { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;
            string command;
            try
            {
                var dataMap = context.MergedJobDataMap;
                BatPath = dataMap.GetString("bat-path");
                ShellName = dataMap.GetString("shell") ?? "cmd.exe";
                OutFolder = dataMap.GetString("out-folder") ?? Path.Combine(Environment.CurrentDirectory, "outs");
                FileName = dataMap.GetString("file-name") ?? "";
                FileFormat = dataMap.GetString("file-format") ?? ".yyyyMMddHHmmss";
                FileExt = dataMap.GetString("file-ext") ?? "txt";
                string fileName = Path.Combine(OutFolder, string.Concat(FileName, DateTime.Now.ToString(FileFormat), ".", FileExt));
                command = File.ReadAllTextAsync(BatPath).Result.Replace("{fileName}", fileName);
                log.Info($"[命令]-- {command}");
                var result = ExecShellCommand(p =>
                {
                    p(command);
                    p("exit 0");
                });
                string message = result ? fileName : "失败";
                log.Info($@"[结果]-- {message}");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return TaskUtil.CompletedTask;
        }

        /// <summary>
        /// 打开控制台执行拼接完成的批处理命令字符串
        /// </summary>
        /// <param name="inputAction">需要执行的命令委托方法：每次调用 <paramref name="inputAction"/> 中的参数都会执行一次</param>
        private bool ExecShellCommand(Action<Action<string>> inputAction)
        {
            bool result = false;
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;

            try
            {
                pro = new Process();
                pro.StartInfo.FileName = ShellName;
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;

                pro.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                pro.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                pro.Start();
                sIn = pro.StandardInput;
                sIn.AutoFlush = true;

                pro.BeginOutputReadLine();
                inputAction(value => sIn.WriteLine(value));

                pro.WaitForExit();
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (pro != null && !pro.HasExited)
                    pro.Kill();

                if (sIn != null)
                    sIn.Close();
                if (sOut != null)
                    sOut.Close();
                if (pro != null)
                    pro.Close();
            }
            return result;
        }
    }
}

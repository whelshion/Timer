using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Timer.WinService.Net4.Utils
{
    public class ShellUtil
    {
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(AppUtil.LoggerRepository.Name, typeof(ShellUtil));

        /// <summary>
        /// 执行输入命令
        /// </summary>
        /// <param name="fileName">cmd.exe/sh/bash等</param>
        /// <param name="inputAction">需要执行的命令委托方法：每次调用 <paramref name="inputAction"/> 中的参数都会执行一次</param>
        /// <param name="wait">是否等待命令执行完成</param>
        public static bool ExecuteCommand(string fileName, Action<Action<string>> inputAction, string workingDirectory, bool wait = true)
        {
            bool result = false;
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    if (workingDirectory != null)
                        process.StartInfo.WorkingDirectory = workingDirectory;

                    process.OutputDataReceived += (sender, e) => Logger.Info(e.Data ?? "Output:-");
                    process.ErrorDataReceived += (sender, e) => Logger.Error(e.Data ?? "Error:-");

                    process.Start();
                    process.StandardInput.AutoFlush = true;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    inputAction(value => process.StandardInput.WriteLine(value));

                    if (wait)
                        process.WaitForExit();

                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    Logger.Error(ex.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 参数化执行命令
        /// </summary>
        /// <param name="fileName">cmd.exe/sh/bash</param>
        /// <param name="arguments">参数</param>
        /// <param name="workingDirectory">工作路径</param>
        /// <param name="wait">是否等待命令执行完成</param>
        /// <returns></returns>
        public static bool ExecuteCommand(string fileName, string arguments, string workingDirectory, bool wait = true)
        {
            bool result = true;
            try
            {
                Logger.Info("ExecuteCommand:" + fileName + " " + arguments);
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.UseShellExecute = false;
                    if (workingDirectory != null)
                        process.StartInfo.WorkingDirectory = workingDirectory;

                    process.StartInfo.CreateNoWindow = true;

                    process.OutputDataReceived += (sender, e) => Logger.Info(e.Data ?? "Output:-");
                    process.ErrorDataReceived += (sender, e) => Logger.Error(e.Data ?? "Error:-");

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (wait)
                        process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                result = false;
                Logger.Error(ex.ToString());
            }
            return result;
        }
    }
}

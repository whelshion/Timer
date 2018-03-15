using log4net;
using MySql.Data.MySqlClient;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer.ShellExecuter.Utils;

namespace Timer.ShellExecuter.Jobs
{
    public class TUQuickAnalysisJob : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(AppSetting.LoggerRepository.Name, typeof(BackupJob));

        public static bool IsActive { get; set; }
        public string ScriptPath { get; private set; }
        public string DbType { get; private set; }
        public string DbConnString { get; private set; }
        public string ShellName { get; private set; }
        public string NoticeApi { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            log.Info($"新周期触发，线程id({Thread.CurrentThread.ManagedThreadId})*************************************************");
            var dataMap = context.MergedJobDataMap;

            ScriptPath = dataMap.GetString("shell-script-path");
            ShellName = dataMap.GetString("shell") ?? "/bin/bash";
            DbType = dataMap.GetString("db-type") ?? "MySql";
            DbConnString = dataMap.GetString("conn-string") ?? "";
            NoticeApi = dataMap.GetString("notice-api") ?? "";

            if (IsActive)
            {
                log.Info("存在执行中任务,跳过该周期!");
            }
            else
            {
                IsActive = true;
                string command = string.Empty;
                string querySql;
                try
                {
                    DbConnection conn;
                    switch (DbType)
                    {
                        case "MsSql":
                            conn = new SqlConnection(DbConnString);
                            querySql = @"select top 1 task_detail_id,ttime,thour,def_cellname,type1,type3 from manager_task_detail where reply='1001' limit 1";
                            break;
                        default:
                            conn = new MySqlConnection(DbConnString);
                            querySql = @"select task_detail_id,ttime,thour,def_cellname,type1,type3 from manager_task_detail where reply='1001' limit 1";
                            break;
                    }
                    using (conn)
                    {
                        try
                        {
                            conn.Open();

                            var cmd = conn.CreateCommand();
                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.CommandText = querySql;
                            cmd.CommandTimeout = 0;
                            var reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                long? task_detail_id = null;
                                while (reader.Read())
                                {
                                    int i = 0;
                                    task_detail_id = reader.GetInt64(i++);
                                    command = File.ReadAllTextAsync(ScriptPath).Result
                                        .Replace("{ttime}", reader.GetString(i++))
                                        .Replace("{thour}", reader.GetString(i++))
                                        .Replace("{phone}", reader.GetString(i++))
                                        .Replace("{type1}", reader.GetString(i++))
                                        .Replace("{type3}", reader.GetString(i++))
                                        ;
                                    break;
                                }
                                conn.Close();

                                log.Info($"[命令]-- {command}");
                                var result = ExecShellCommand(p =>
                                {
                                    p(command);
                                    p("exit 0");
                                });
                                string message = result ? "成功" : "失败";
                                log.Info($@"[结果]-- {message}");

                                while (true)
                                {
                                    conn.Open();
                                    cmd.CommandText = $"select reply from manager_task_detail where task_detail_id={task_detail_id}";
                                    string analysisResult = (string)cmd.ExecuteScalar();
                                    conn.Close();
                                    if (analysisResult != "1001")
                                    {
                                        IsActive = false;
                                        log.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi, timeout: 60));
                                        break;
                                    }
                                    Thread.Sleep(30 * 1000);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                        finally
                        {
                            conn.Close();
                            log.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi, timeout: 60));
                        }
                        Thread.Sleep(5000);//测试存在执行中任务
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
                finally
                {
                    IsActive = false;
                }
            }
            log.Info($"周期结束，线程id({Thread.CurrentThread.ManagedThreadId})*************************************************)");
            return TaskUtil.CompletedTask;
        }

        /// <summary>
        /// 打开控制台执行拼接完成的批处理命令字符串
        /// </summary>
        /// <param name="inputAction">需要执行的命令委托方法：每次调用 <paramref name="inputAction"/> 中的参数都会执行一次</param>
        private bool ExecShellCommand(Action<Action<string>> inputAction)
        {
            bool result = false;
            StreamWriter sIn = null;
            StreamReader sOut = null;
            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo.FileName = ShellName;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    //pro.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    //pro.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.OutputDataReceived += (sender, e) => log.Info(e.Data ?? "Output:-");
                    process.ErrorDataReceived += (sender, e) => log.Error(e.Data ?? "Error:-");

                    process.Start();
                    sIn = process.StandardInput;
                    sIn.AutoFlush = true;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    inputAction(value => sIn.WriteLine(value));

                    process.WaitForExit();
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    throw ex;
                }
                finally
                {
                    if (sIn != null)
                        sIn.Close();
                    if (sOut != null)
                        sOut.Close();
                }
            }
            return result;
        }
    }
}

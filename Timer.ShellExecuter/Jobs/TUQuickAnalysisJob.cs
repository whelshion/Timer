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
        private static readonly ILog log = LogManager.GetLogger(AppSetting.LoggerRepository.Name, typeof(TUQuickAnalysisJob));

        public static bool IsActive { get; set; }
        public string ScriptPath { get; private set; }
        public string DbType { get; private set; }
        public string DbConnString { get; private set; }
        public string ShellName { get; private set; }
        public string NoticeApi { get; private set; }
        public string NoticeApi2 { get; private set; }
        public string AfterShellSql { get; private set; }

        public Task Execute(IJobExecutionContext context)
        {
            log.Info($"新周期触发(线程ID:{Thread.CurrentThread.ManagedThreadId})*************************************************");
            var dataMap = context.MergedJobDataMap;

            ScriptPath = dataMap.GetString("shell-script-path");
            ShellName = dataMap.GetString("shell") ?? "/bin/bash";
            DbType = dataMap.GetString("db-type") ?? "MySql";
            DbConnString = dataMap.GetString("conn-string") ?? "";
            NoticeApi = dataMap.GetString("notice-api") ?? "";
            NoticeApi2 = dataMap.GetString("notice-api-2") ?? "";
            AfterShellSql = dataMap.GetString("after-shell-sql") ?? "";

            if (IsActive)
            {
                log.Info("存在执行中任务,跳过该周期!");
            }
            else
            {
                IsActive = true;
                string command = string.Empty;
                string querySql;
                long? task_detail_id = 0;
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
                                string arguments = string.Empty;

                                while (reader.Read())
                                {
                                    int i = 0;
                                    task_detail_id = reader.GetInt64(0);
                                    //command = File.ReadAllTextAsync(ScriptPath).Result
                                    //    .Replace("{ttime}", reader.GetString(i++))
                                    //    .Replace("{thour}", reader.GetString(i++))
                                    //    .Replace("{phone}", reader.GetString(i++))
                                    //    .Replace("{type1}", reader.GetString(i++))
                                    //    .Replace("{type3}", reader.GetString(i++))
                                    //    ;
                                    string type3 = reader.GetString(5);
                                    int type3Value = type3.Contains("未接通") ? 1 :
                                        type3.Contains("掉话") ? 2 :
                                        type3.Contains("切换失败") ? 3 :
                                        throw new ArgumentException($"TYPE3不在范围内:{type3}");

                                    log.Info($"查询到工单:{reader.GetString(0)} {reader.GetString(1)} {reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)} {type3}");
                                    arguments = $"{ShellName} {reader.GetString(1)} {reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)} {type3Value}";
                                    break;
                                }
                                conn.Close();

                                //var result = ExecShellCommand(p =>
                                //{
                                //    p(command);
                                //    p("exit 0");
                                //});
                                var result = ExecuteCommand("/bin/bash", arguments, null);
                                //string message = result ? "成功" : "失败";
                                //log.Info($@"[SHELL结果]-- {message}");

                                log.Info($"[AfterShellSql:]-- {AfterShellSql}");
                                if (!string.IsNullOrEmpty(AfterShellSql))
                                {
                                    conn.Open();
                                    cmd.CommandText = AfterShellSql;
                                    try
                                    {
                                        log.Info(cmd.ExecuteNonQuery());
                                    }
                                    catch { }
                                    conn.Close();
                                }

                                HttpUtil.HttpGet(NoticeApi2 + $"?task_detail_id={task_detail_id}", timeout: 60);

                                while (true)
                                {
                                    conn.Open();
                                    cmd.CommandText = $"select reply from manager_task_detail where task_detail_id={task_detail_id}";
                                    string analysisResult = (string)cmd.ExecuteScalar();
                                    conn.Close();
                                    if (analysisResult != "1001")
                                    {
                                        IsActive = false;
                                        log.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi + $"?id={task_detail_id}", timeout: 60));
                                        break;
                                    }
                                    Thread.Sleep(1000);
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
                            log.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi + $"?id={task_detail_id}", timeout: 60));
                        }
                        //Thread.Sleep(10*1000);//测试存在执行中任务
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
            log.Info($"周期结束(线程ID:{Thread.CurrentThread.ManagedThreadId})*************************************************)");
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

        public bool ExecuteCommand(string filename, string arguments, string workingDirectory, bool wait = true)
        {
            bool result = true;
            try
            {
                log.Info("ExecuteCommand:" + filename + " " + arguments);
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = filename;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.UseShellExecute = false;
                    if (workingDirectory != null)
                    {
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    }
                    process.StartInfo.CreateNoWindow = true;
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            //if (filename != YARNCmd)
                            {
                                log.Info(e.Data);
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            //if (filename != YARNCmd)
                            {
                                log.Info(e.Data);
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    if (wait)
                    {
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            return result;
        }

    }
}

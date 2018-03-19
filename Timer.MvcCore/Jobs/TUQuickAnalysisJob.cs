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
using Timer.MvcCore.Utils;

namespace Timer.MvcCore.Jobs
{
    public class TuQuickAnalysisJob : BaseJob
    {
        public TuQuickAnalysisJob() : base(typeof(TuQuickAnalysisJob)) { }
        public static bool IsActive { get; set; }
        public string ScriptPath { get; private set; }
        public string DbType { get; private set; }
        public string DbConnString { get; private set; }
        public string ShellName { get; private set; }
        public string NoticeApi { get; private set; }
        public string NoticeApi2 { get; private set; }
        public string AfterShellSql { get; private set; }


        public bool ExecuteCommand(string filename, string arguments, string workingDirectory, bool wait = true)
        {
            bool result = true;
            try
            {
                Logger.Info("ExecuteCommand:" + filename + " " + arguments);
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
                                Logger.Info(e.Data);
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            //if (filename != YARNCmd)
                            {
                                Logger.Info(e.Data);
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

        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            Logger.Info($"新周期触发(线程ID:{Thread.CurrentThread.ManagedThreadId})*************************************************");
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
                Logger.Info("存在执行中任务,跳过该周期!");
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

                                    Logger.Info($"查询到工单:{reader.GetString(0)} {reader.GetString(1)} {reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)} {type3}");
                                    arguments = $"{ShellName} {reader.GetString(1)} {reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)} {type3Value}";
                                    break;
                                }
                                conn.Close();
                                var result = ShellUtil.ExecuteCommand("/bin/bash", arguments, null);
                                Logger.Info($"[AfterShellSql:]-- {AfterShellSql}");
                                if (!string.IsNullOrEmpty(AfterShellSql))
                                {
                                    conn.Open();
                                    cmd.CommandText = AfterShellSql;
                                    try
                                    {
                                        Logger.Info(cmd.ExecuteNonQuery());
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
                                        Logger.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi + $"?id={task_detail_id}", timeout: 60));
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                        finally
                        {
                            conn.Close();
                            Logger.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi + $"?id={task_detail_id}", timeout: 60));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    IsActive = false;
                }
            }
            Logger.Info($"周期结束(线程ID:{Thread.CurrentThread.ManagedThreadId})*************************************************)");
            return TaskUtil.CompletedTask;
        }
    }
}

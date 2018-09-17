using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Timer.Web.Core.Utils;
using System.Data.Common;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Text;
using System.Diagnostics;

namespace Timer.Web.Core.Jobs
{
    [Description("批量TOP用户UE_MR查询任务")]
    public class TuBatchAnalysisJob:BaseJob
    {
        public static bool IsActive { get; private set; }
        public string ScriptPath { get; private set; }
        public string DbType { get; private set; }
        public string DbConnString { get; private set; }
        public string ShellName { get; private set; }
        
        public string BeforeShellSql { get; private set; }

        public string AfterShellSql { get; private set; }
        public string AutoDelete { get; private set; }
        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;

            ScriptPath = dataMap.GetString("shell-script-path");
            ShellName = dataMap.GetString("shell") ?? "/bin/bash";
            DbType = dataMap.GetString("db-type") ?? "MySql";
            DbConnString = dataMap.GetString("conn-string") ?? "";
           
            AfterShellSql = dataMap.GetString("after-shell-sql") ?? "";
            BeforeShellSql = dataMap.GetString("before-shell-sql") ?? "";
            AutoDelete = dataMap.GetString("auto-delete")??"";
            var logService = new LogService(DbType, DbConnString);
            var topService = new TopService(DbType, DbConnString);
            Stopwatch stopwatch = new Stopwatch();

            if (IsActive)
            {
                Logger.Info("存在执行中任务,跳过该周期!");
            }
            else
            {
                IsActive = true;
                string command = string.Empty;
                string querySql=string.Empty;
                long? task_id = 0;
                string ttime = string.Empty;
                string thour = string.Empty;
                string phone_number = string.Empty;
                string tableName = string.Empty;
                string filePath = string.Empty;
                string operate_type = "5";
                int step_no = 1;
                try
                {
                    DbConnection conn;
                    switch(DbType)
                    {
                        case "MsSql":
                            conn = new SqlConnection(DbConnString);
                            //querySql = @"select task_id,list_order,UTCstr,logtime,logmillisecond,MSISDN from sig_ue_mr_top200_plan";
                            break;
                        default:
                            conn = new MySqlConnection(DbConnString);
                            //querySql = @"select task_id,list_order,UTCstr,logtime,logmillisecond,MSISDN from sig_ue_mr_top200_plan";
                            break;
                    }
                    using (conn)
                    {
                        try
                        {                           
                            conn.Open();
                            var cmd = conn.CreateCommand();
                            cmd.CommandType = System.Data.CommandType.Text;
                            TableEntity.sig_ue_mr_top200_job sig_ue_mr_top200_job = new TableEntity.sig_ue_mr_top200_job();
                            sig_ue_mr_top200_job = topService.Query("select * from sig_ue_mr_top200_job where completeTime is null or completeTime='' order by createTime limit 1");
                            if (sig_ue_mr_top200_job != null)
                            {
                                //更新任务状态
                                sig_ue_mr_top200_job.state = ExecStatus.运算中.ToString();
                                topService.Update(sig_ue_mr_top200_job);

                                cmd.CommandText = @"drop table if exists sig_ue_mr_top200_plan_tmp";
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = @"create table sig_ue_mr_top200_plan_tmp(task_id int,list_order int,
                                                                  UTCstr nvarchar(20),
                                                                  logtime nvarchar(22),
                                                                  logmillisecond nvarchar(10),
                                                                  MSISDN nvarchar(15))";
                                cmd.ExecuteNonQuery();
                                //选取数据到临时表
                                cmd.CommandText = string.Format(@"insert into sig_ue_mr_top200_plan_tmp select task_id,list_order,UTCstr,logtime,logmillisecond,MSISDN from sig_ue_mr_top200_plan where task_id={0}", sig_ue_mr_top200_job.task_id);
                                cmd.ExecuteNonQuery();


                                querySql = string.Format(@"select task_id,list_order,UTCstr,logtime,logmillisecond,MSISDN from sig_ue_mr_top200_plan_tmp where task_id={0}", sig_ue_mr_top200_job.task_id);
                                cmd.CommandText = querySql;
                                cmd.CommandTimeout = 0;
                                var reader = cmd.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    List<string> arguments = new List<string>() { };
                                    StringBuilder str = new StringBuilder();
                                    while (reader.Read())
                                    {
                                        task_id = reader.GetInt64(0);
                                        str.Append(reader.GetString(5));
                                        str.Append(",");
                                        str.Append(reader.GetString(2));
                                        str.Append("|");
                                    }
                                    str.Remove(str.Length - 1, 1);
                                    phone_number = str.ToString();
                                    ShellName.Split(";").ToList().ForEach(shell =>
                                    {
                                        arguments.Add($"{shell} {ttime} {thour} {phone_number} {filePath} {operate_type}");
                                    });
                                    conn.Close();

                                    Logger.Info($"[BeforeShellSql:]-- {BeforeShellSql}");
                                    if (!string.IsNullOrEmpty(BeforeShellSql))
                                    {
                                        conn.Open();
                                        cmd.CommandText = BeforeShellSql.Replace("@select_date", ttime);//替换日期占位符


                                        //记录日志到数据表
                                        var step_1 = logService.Insert(new TableEntity.SingleUserAnalysisHistoryInfo()
                                        {
                                            col_create_time = DateTime.Now,
                                            error_msg = "",//异常信息
                                            run_status = RunStatus.执行中.ToString(),
                                            spend_time = 0,//耗时
                                            step_name = "（BeforeShellSql）执行shell脚本之前的sql脚本",//步骤名称
                                            step_no = step_no,//步骤编号
                                            step_remark = "",//备注
                                            task_detail_id = task_id ?? 0//task_id
                                        });
                                        stopwatch.Restart();//计时器开始
                                        int asqResult = 0;

                                        try
                                        {
                                            asqResult = cmd.ExecuteNonQuery();
                                            Logger.Info($" {asqResult} 行受影响");
                                        }
                                        catch (Exception ex)
                                        {
                                            step_1.error_msg = ex.Message;
                                        }

                                        stopwatch.Stop();//计时器停止
                                        step_1.run_status = RunStatus.已执行.ToString();
                                        step_1.step_remark = $" {asqResult} 行受影响";
                                        step_1.spend_time = (int)stopwatch.Elapsed.TotalSeconds;
                                        logService.Update(step_1);

                                        conn.Close();
                                    }
                                    Logger.Info($"--------------开始执行SHELL命令--------------");

                                    arguments.ForEach(argument =>
                                    {
                                        step_no++;
                                        //执行shell脚本
                                        Logger.Info($"/bin/bash {argument}");
                                        //记录日志到数据表
                                        var step_2 = logService.Insert(new TableEntity.SingleUserAnalysisHistoryInfo()
                                        {
                                            col_create_time = DateTime.Now,
                                            error_msg = "",//异常信息
                                            run_status = RunStatus.执行中.ToString(),
                                            spend_time = 0,//耗时
                                            step_name = "执行shell脚本，脚本参数" + argument,//步骤名称
                                            step_no = step_no,//步骤编号
                                            step_remark = "",//备注
                                            task_detail_id = task_id ?? 0//task_id
                                        });
                                        stopwatch.Restart();//计时器开始

                                        var result = ShellUtil.ExecuteCommand("/bin/bash", argument, null);

                                        stopwatch.Stop();//计时器停止
                                        step_2.run_status = RunStatus.已执行.ToString();
                                        step_2.step_remark += "执行结果：" + (result.Item1 == true ? "无错误" : string.Concat("有错误,", result.Item2));
                                        step_2.spend_time = (int)stopwatch.Elapsed.TotalSeconds;
                                        logService.Update(step_2);
                                    });
                                    Logger.Info($"--------------执行SHELL命令完成--------------");

                                    Logger.Info($"[AfterShellSql:]-- {AfterShellSql}");
                                    if (!string.IsNullOrEmpty(AfterShellSql))
                                    {
                                        conn.Open();
                                        cmd.CommandText = AfterShellSql.Replace("@select_date", ttime).Replace("@select_hour", thour);//替换日期占位符

                                        //记录日志到数据表
                                        var step_3 = logService.Insert(new TableEntity.SingleUserAnalysisHistoryInfo()
                                        {
                                            col_create_time = DateTime.Now,
                                            error_msg = "",//异常信息
                                            run_status = RunStatus.执行中.ToString(),
                                            spend_time = 0,//耗时
                                            step_name = "（AfterShellSql）执行shell脚本之后的sql脚本",//步骤名称
                                            step_no = step_no,//步骤编号
                                            step_remark = "",//备注
                                            task_detail_id = task_id ?? 0//task_id
                                        });
                                        stopwatch.Restart();//计时器开始
                                        int asqResult = 0;

                                        try
                                        {                                   
                                            asqResult = cmd.ExecuteNonQuery();
                                            Logger.Info($" {asqResult} 行受影响");
                                        }
                                        catch (Exception ex)
                                        {
                                            step_3.error_msg = ex.Message;
                                        }

                                        stopwatch.Stop();//计时器停止
                                        step_3.run_status = RunStatus.已执行.ToString();
                                        step_3.step_remark = $" {asqResult} 行受影响";
                                        step_3.spend_time = (int)stopwatch.Elapsed.TotalSeconds;
                                        logService.Update(step_3);

                                        conn.Close();
                                    }

                                    step_no++;
                                    //记录日志到数据表
                                    var step_4 = logService.Insert(new TableEntity.SingleUserAnalysisHistoryInfo()
                                    {
                                        col_create_time = DateTime.Now,
                                        error_msg = "",//异常信息
                                        run_status = RunStatus.执行中.ToString(),
                                        spend_time = 0,//耗时
                                        step_name = "查询完成，并更新任务状态",//步骤名称
                                        step_no = step_no,//步骤编号
                                        step_remark = "",//备注
                                        task_detail_id = task_id ?? 0//task_id
                                    });
                                    stopwatch.Restart();//计时器开始
                                    sig_ue_mr_top200_job.completeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms");
                                    sig_ue_mr_top200_job.state = ExecStatus.已完成.ToString();
                                    topService.Update(sig_ue_mr_top200_job);
                                    stopwatch.Stop();//计时器停止
                                    step_4.run_status = RunStatus.已执行.ToString();
                                    step_4.step_remark = "Top200用户批量UE_MR查询完成";
                                    step_4.spend_time = (int)stopwatch.Elapsed.TotalSeconds;
                                    logService.Update(step_4);
                                    if(!string.IsNullOrEmpty(AutoDelete))
                                    {
                                        Logger.Info($"[DeleteOldData:task_id]-- {task_id}");
                                        conn.Open();
                                        cmd.CommandText = string.Format(@"delete from sig_ue_mr_top200_plan where task_id={0}",task_id);
                                        cmd.ExecuteNonQuery();
                                        Logger.Info($"[DeleteOldData:task_id]--completed");
                                    }
                                }
                            }       
                           
                        }
                        catch(Exception ex)
                        {
                            Logger.Error(ex);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    IsActive = false;
                }
            }
            return TaskUtil.CompletedTask;
        }
    }
}

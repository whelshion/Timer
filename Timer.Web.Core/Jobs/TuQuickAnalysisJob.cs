using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Quartz;
using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Jobs
{
    [Description("TOP用户快速分析任务")]
    public class TuQuickAnalysisJob : BaseJob
    {
        public static bool IsActive { get; private set; }
        public string ScriptPath { get; private set; }
        public string DbType { get; private set; }
        public string DbConnString { get; private set; }
        public string ShellName { get; private set; }
        public string NoticeApi { get; private set; }
        public string NoticeApi2 { get; private set; }
        public string BeforeShellSql { get; private set; }

        public string AfterShellSql { get; private set; }
        protected override Task ExecuteJob(IJobExecutionContext context)
        {



            var dataMap = context.MergedJobDataMap;

            ScriptPath = dataMap.GetString("shell-script-path");
            ShellName = dataMap.GetString("shell") ?? "/bin/bash";
            DbType = dataMap.GetString("db-type") ?? "MySql";
            DbConnString = dataMap.GetString("conn-string") ?? "";
            NoticeApi = dataMap.GetString("notice-api") ?? "";
            NoticeApi2 = dataMap.GetString("notice-api-2") ?? "";
            AfterShellSql = dataMap.GetString("after-shell-sql") ?? "";
            BeforeShellSql = dataMap.GetString("before-shell-sql") ?? "";
            var logService = new LogService(DbType, DbConnString);
            Stopwatch stopwatch = new Stopwatch();

            //如果表还没初始化
            if (TableInit.TableInit.IsInit == false)
            {
                TableInit.TableInit.Init(DbType, DbConnString);
                TableInit.TableInit.IsInit = true;
            }


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
                string ttime = string.Empty;
                string thour = string.Empty;
                string phone_number = string.Empty;
                string type1 = string.Empty;
                string type3 = string.Empty;
                int step_no = 1;

                // Logger.Info($"任务数据--{Environment.NewLine}ShellName={ShellName}{Environment.NewLine}ScriptPath={ScriptPath}{Environment.NewLine}DbType={DbType}{Environment.NewLine}DbConnString={DbConnString}{Environment.NewLine}BeforeShellSql={BeforeShellSql}{Environment.NewLine}AfterShellSql={AfterShellSql}{Environment.NewLine}NoticeApi={NoticeApi}{Environment.NewLine}NoticeApi2={NoticeApi2}");
                try
                {
                    DbConnection conn;
                    switch (DbType)
                    {
                        case "MsSql":
                            conn = new SqlConnection(DbConnString);
                            querySql = @"select top 1 task_detail_id,ttime,thour,def_cellname,type1,type3 from manager_task_detail where reply='1001' ";
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
                                List<string> arguments = new List<string>() { };

                                while (reader.Read())
                                {
                                    //int i = 0;
                                    task_detail_id = reader.GetInt64(0);
                                    ttime = reader.GetString(1);
                                    thour = reader.GetString(2);
                                    phone_number = reader.GetString(3);
                                    type1 = reader.GetString(4);
                                    type3 = reader.GetString(5);
                                    int type3Value = type3.Contains("未接通") ? 1 : type3.Contains("掉话") ? 2 : type3.ToUpper().Contains("ESRVCC切换失败") ? 4 : type3.Contains("切换失败") ? 3 : throw new ArgumentOutOfRangeException($"TYPE3不在范围内:{type3}");

                                    Logger.Info($"查询到工单{{task_detail_id:{task_detail_id},ttime:{ttime},thour:{thour},phone_number:{phone_number},type1:{type1},type3:{type3}}}");

                                    ShellName.Split(";").ToList().ForEach(shell =>
                                    {
                                        arguments.Add($"{ShellName} {ttime} {thour} {phone_number} {type1} {type3Value}");
                                    });

                                    break;
                                }
                                conn.Close();


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
                                        task_detail_id = task_detail_id ?? 0//工单id
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
                                        task_detail_id = task_detail_id ?? 0//工单id
                                    });
                                    stopwatch.Restart();//计时器开始

                                    var result = ShellUtil.ExecuteCommand("/bin/bash", argument, null);

                                    stopwatch.Stop();//计时器停止
                                    step_2.run_status = RunStatus.已执行.ToString();
                                    step_2.step_remark += "执行结果：" + (result == true ? "无错误" : "有错误，请查看日志文件");
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
                                        task_detail_id = task_detail_id ?? 0//工单id
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
                                    step_name = "生成工单结果 并更新reply为1002",//步骤名称
                                    step_no = step_no,//步骤编号
                                    step_remark = "",//备注
                                    task_detail_id = task_detail_id ?? 0//工单id
                                });
                                stopwatch.Restart();//计时器开始

                                //生成工单结果 并更新reply为1002
                                //NoticeApi2 : http://120.76.26.161:5000/api/WorkorderService/BuildCellQuestion 
                                var httpResult = HttpUtil.HttpGet(NoticeApi2 + $"?task_detail_id={task_detail_id}", timeout: 120);
                                Logger.Info("生成工单结果:" + httpResult);

                                stopwatch.Stop();//计时器停止
                                step_4.run_status = RunStatus.已执行.ToString();
                                step_4.step_remark = httpResult;
                                step_4.spend_time = (int)stopwatch.Elapsed.TotalSeconds;
                                logService.Update(step_4);









                                //通知专家系统网站该工单已经处理完毕
                                //10秒/次*30次无结果则跳过，直接通知专家系统
                                int i = 0;
                                while (i++ < 30)
                                {
                                    conn.Open();
                                    cmd.CommandText = $"select reply from manager_task_detail where task_detail_id={task_detail_id}";
                                    string analysisResult = (string)cmd.ExecuteScalar();
                                    conn.Close();
                                    if (analysisResult != "1001") break;
                                    Thread.Sleep(1000 * 10);
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
                            //NoticeApi : http://120.76.26.161:5000/TaskManagement/OnTuAnalysisFinished
                            if (task_detail_id.GetValueOrDefault() != 0)
                                Logger.Info("通知专家系统:" + HttpUtil.HttpGet(NoticeApi + $"?id={task_detail_id}", timeout: 60));
                        }
                        //Thread.Sleep(10*1000);//测试存在执行中任务
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
            return TaskUtil.CompletedTask;
        }
    }
}

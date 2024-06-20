using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.Windows;
using FtpBackup.Config;
using FtpBackup.Views;

namespace FtpBackup.Utils;

public class BackupScheduler
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public static async Task ScheduleDailyBackup(List<string> filePaths)
    {
        try
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            var jobData = new JobDataMap
                {
                    { "FilePaths", filePaths.ConvertToString() }
                };

            IJobDetail job = JobBuilder.Create<BackupJob>()
                .UsingJobData(jobData)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error scheduling backup");
        }
    }
}

public class BackupJob : IJob
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            string? filePaths = context.JobDetail.JobDataMap.GetString("FilePaths");
            if (filePaths != null)
            {
                List<string> filePathList = filePaths.ConvertToStringList();
                MessageBox.Show($"{filePathList.Count()}");
                if (filePathList != null)
                    await MainWindow.Instance.ftpBackupProcess.BackupFiles(filePathList);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error executing backup job");
        }
    }
}
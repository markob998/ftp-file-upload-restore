using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

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
                    { "FilePaths", JsonConvert.SerializeObject(filePaths) }
                };

            IJobDetail job = JobBuilder.Create<BackupJob>()
                .UsingJobData(jobData)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(5)
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
            string? filePathsJson = context.JobDetail.JobDataMap.GetString("FilePaths");
            if (filePathsJson != null)
            {
                List<string>? filePaths = JsonConvert.DeserializeObject<List<string>>(filePathsJson);
                var backupProcess = new FtpBackupProcess();
                if (filePaths != null)
                    await backupProcess.BackupFiles(filePaths);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error executing backup job");
        }
    }
}
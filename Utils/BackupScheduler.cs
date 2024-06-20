using Quartz;
using Quartz.Impl;
using System.Threading.Tasks;

namespace FtpBackup.Utils
{
    public class BackupScheduler
    {
        public static async Task ScheduleDailyBackup(string folderPath)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<BackupJob>()
                .UsingJobData("FolderPath", folderPath)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public class BackupJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            string folderPath = context.JobDetail.JobDataMap.GetString("FolderPath");
            var backupProcess = new FtpBackupProcess();
            // await backupProcess.BackupFolder(folderPath);
        }
    }
}


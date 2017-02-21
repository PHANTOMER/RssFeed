using Core.Common;
using Core.Extensions;
using Quartz;

namespace Core.Jobs
{
    public interface IJobBuilder
    {
        void Build(IScheduler scheduler);
    }

    class FeedScrapingJobBuilder : IJobBuilder
    {
        private readonly IConfigurationProvider _configuration;

        public FeedScrapingJobBuilder(IConfigurationProvider configuration)
        {
            _configuration = configuration;
        }
        
        public void Build(IScheduler scheduler)
        {
            int interval = _configuration.AppSettings["FeedScrapingJobInterval"].ToInt();

            var job = JobBuilder.Create<FeedScrapingJob>()
                .WithIdentity("FeedScrapingJob")
                .RequestRecovery()
                .Build();

            // Associate a trigger with the Job

            var trigger = TriggerBuilder.Create()
                .WithIdentity("FeedScrapingJobTrigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(interval).RepeatForever())
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}

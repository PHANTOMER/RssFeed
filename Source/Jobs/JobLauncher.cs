using System.Collections.Generic;
using NLog;
using Quartz;

namespace Core.Jobs
{
    public class JobLauncher
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IEnumerable<IJobBuilder> _jobBuilders;
        private IScheduler _scheduler;

        public JobLauncher(ISchedulerFactory schedulerFactory, IEnumerable<IJobBuilder> jobBuilders)
        {
            _schedulerFactory = schedulerFactory;
            _jobBuilders = jobBuilders;
        }

        public void Init()
        {
            _logger.Trace("Scheduling jobs...");

            _scheduler = _schedulerFactory.Create();
            
            if (!_scheduler.IsStarted)
                _scheduler.Start();

            // Add all Scheduled Jobs here
            foreach (var jobBuilder in _jobBuilders)
            {
                jobBuilder.Build(_scheduler);
            }
        }

        public void Stop()
        {
            if (_scheduler.IsShutdown)
                return;

            _logger.Trace("Shutting down all jobs...");
            _scheduler.Clear();
            _scheduler.Shutdown(false);
        }
    }
}

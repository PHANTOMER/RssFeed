using System;
using Core.Autofac;
using NLog;
using Quartz;
using Quartz.Impl;

namespace Core.Jobs
{
    public interface ISchedulerFactory
    {
        IScheduler Create();
    }

    class SchedulerFactory : ISchedulerFactory
    {
        private readonly MainModule _module;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public SchedulerFactory(MainModule module)
        {
            _module = module;
        }

        public IScheduler Create()
        {
            try
            {
                // Get a reference to the scheduler
                var sf = new StdSchedulerFactory();
                var scheduler = sf.GetScheduler();
                var scope = AutoFacCore.InitScope(_module);
                scheduler.JobFactory = new AutoFacJobFactory(scope);
                return scheduler;
            }
            catch (Exception ex)
            {
                _logger.Fatal("Scheduler not available: '{0}'", ex.Message);
                throw;
            }
        }
    }
}

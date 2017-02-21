using System;
using Autofac;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Core.Autofac
{
    class AutoFacJobFactory : SimpleJobFactory
    {
        private readonly ILifetimeScope _container;

        public AutoFacJobFactory(ILifetimeScope container)
        {
            this._container = container;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return (IJob)_container.Resolve(bundle.JobDetail.JobType); // will inject dependencies that the job requires
            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    $"Problem while instantiating job '{bundle.JobDetail.Key}' from the AutoFacJobFactory.", e);
            }
        }
    }
}

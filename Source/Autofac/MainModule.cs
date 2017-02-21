using Autofac;
using Autofac.Core;
using Core.Common;
using Core.DataAccess;
using Core.Jobs;
using Core.Scraper;

namespace Core.Autofac
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppConfigurationProvider>().As<IConfigurationProvider>();
            builder.RegisterType<JobLauncher>().AsSelf();
            builder.RegisterType<FeedScrapingJob>().AsSelf();
            builder.RegisterType<FeedScrapingJobBuilder>().As<IJobBuilder>();
            builder.Register((c, p) => new SchedulerFactory(this)).As<ISchedulerFactory>();
            builder.RegisterType<ScraperFactory>().AsSelf();

            builder.RegisterType<FeedDataService>().As<IFeedDataService>();
            builder.RegisterType<MongoContext>().As<IDbContext>();

            builder.RegisterAdapter<IConfigurationProvider, ContextCreator>(config =>
                new ContextCreator(config.ConnectionStrings["mongodb"]));
        }
    }
}

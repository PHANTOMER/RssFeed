using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using Core.Common;
using NLog;
using RssFeed.Configuration;
using RssFeed.Controllers;
using RssFeed.Services;
using Module = Autofac.Module;

namespace RssFeed.AutoFac
{
    class WebModule : Module
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        protected override void Load(ContainerBuilder builder)
        {
            _logger.Trace("Registering api controllers...");

            builder.RegisterApiControllers(typeof(FeedController).Assembly);
            //builder.RegisterType<WebConfigurationProvider>().As<IConfigurationProvider>();
            builder.RegisterType<RssFeedProvider>().As<IRssFeedProvider>();
        }
    }
}

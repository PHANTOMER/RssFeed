using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Autofac.Integration.WebApi;
using Core.Autofac;
using Microsoft.Owin;
using NLog;
using Owin;
using RssFeed.AutoFac;

[assembly: OwinStartup(typeof(RssFeed.Startup))]

namespace RssFeed
{
    public class Startup
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            _logger.Trace("Configuring service...");

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);

            var scope = AutoFacCore.InitScope(new MainModule(), new WebModule());
            
            appBuilder.Use<GlobalExceptionMiddleware>();
            appBuilder.Use((context, next) =>
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.SuppressFormsAuthenticationRedirect = true;
                }

                return next.Invoke();
            });


            config.DependencyResolver = new AutofacWebApiDependencyResolver(scope);
            appBuilder.UseAutofacMiddleware(scope);
            appBuilder.UseWebApi(config);
        }
    }

    public class GlobalExceptionMiddleware : OwinMiddleware
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public GlobalExceptionMiddleware(OwinMiddleware next)
            : base(next)
        { }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                Debugger.Launch();
                _logger.Error(ex);
            }
        }
    }
}

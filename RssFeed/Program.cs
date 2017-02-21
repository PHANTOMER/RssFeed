using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Autofac;
using Core.Autofac;
using Core.Extensions;
using Core.Jobs;
using RssFeed.AutoFac;
using RssFeed.Service;

namespace RssFeed
{
    class Program
    {
        [STAThread]

        private static void Main()
        {
            bool debug = IsDebugMode();
            if (debug)
            {
                Debugger.Launch();
            }

            
            using (RssService rssService = GetService())
            {
                if (IsConsoleMode())
                {
                    rssService.Open();

                    ConsoleKeyInfo info = Console.ReadKey(true);
                    while (info.Key != ConsoleKey.Enter)
                    {
                        info = Console.ReadKey(true);
                    }
                    
                    rssService.Close();
                }
                else
                {
                    ServiceBase.Run(rssService);
                }
            }
        }

        private static bool IsDebugMode()
        {
            var args = Environment.GetCommandLineArgs();
            return (args.Length > 0 && args.Any(x => x.ToLower() == "debug"));
        }

        private static bool IsConsoleMode()
        {
            return ConfigurationManager.AppSettings["IsConsole"].ToBoolean();
        }

        private static RssService GetService()
        {
            var serviceName = GetServiceNameFromConfiguration();
            string hostName = ConfigurationManager.AppSettings["Host"];
            int port = ConfigurationManager.AppSettings["Port"].ToInt();
            bool useSsl = ConfigurationManager.AppSettings["UseSsl"].ToBoolean();

            var scope = AutoFacCore.InitScope(new MainModule());
            var jobLauncher = scope.Resolve<JobLauncher>();

            return new RssService(serviceName, $"http{(useSsl ? "s" : "")}://{hostName}:{port}", jobLauncher);
        }

        private static string GetServiceNameFromConfiguration()
        {
            // READ THIS: Get the service name from App.Config
            var serviceName = ConfigurationManager.AppSettings["ServiceName"];

            if (string.IsNullOrEmpty(serviceName))
                serviceName = "NotSpecified";

            return serviceName;
        }
    }
}

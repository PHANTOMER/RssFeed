using System;
using System.IO;
using System.Threading.Tasks;
using Core.Common;
using Core.DataAccess;
using Core.Models;
using Core.Scraper;
using NLog;
using Quartz;

namespace Core.Jobs
{
    class FeedScrapingJob : IJob
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IConfigurationProvider _configuration;
        private readonly ScraperFactory _scraperFactory;
        private readonly IFeedDataService _dataService;

        public FeedScrapingJob(IConfigurationProvider configuration, ScraperFactory scraperFactory, IFeedDataService dataService)
        {
            _configuration = configuration;
            _scraperFactory = scraperFactory;
            _dataService = dataService;
        }

        public void Execute(IJobExecutionContext context)
        {
            _logger.Trace("Starting scraping job");

            ScraperConfig config = new ScraperConfig
            {
                UserName = _configuration.AppSettings["UserName"],
                Password = _configuration.AppSettings["Password"],
                Url = _configuration.AppSettings["Url"],
                LoginUrl = _configuration.AppSettings["LoginUrl"],
                ApiUrl = _configuration.AppSettings["FeedUrl"],
                Proxies = new []{ "91.142.84.182:3128" } //TODO File.ReadAllLines(@"Data\fastproxies.txt")
            };

            using (NewsFeedScraper scraper = _scraperFactory.Create(config))
            {
                try
                {
                    scraper.GoToDashboard(config);
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                    Feed feed = scraper.TryScrapeFeed(config);

                    if (feed != null && feed.Items != null)
                    {
                        foreach (var item in feed.Items)
                        {
                            _logger.Trace($"{item.Id} - {item.JobString} - {item.Title} - {item.Url} - {item.Description}");
                        }

                        _dataService.UpdateFeedAsync(feed).Wait();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    // take screenshot
                    scraper.TakeScreenshot();
                    scraper.SaveSource();
                }
            }
        }
    }
}

using System;
using System.Net;
using Core.Common;
using Core.Extensions;
using Core.Models;
using Core.Scraper;
using FluentAssertions;
using NLog;
using NUnit.Framework;

namespace Tests.UnitTests
{
    [TestFixture]
    public class ScraperTests
    {
        private IConfigurationProvider _configuration;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        [TestFixtureSetUp]
        public void GlobalSetup()
        {
            _configuration = new AppConfigurationProvider();
        }

        [TestFixtureTearDown]
        public void GlobalTeardown()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
        }

        [Test]
        public void What_State_Expect()
        {
            using (var client = new WebClient())
            {
                string data = client.DownloadString("https://www.freelancer.com/rss.xml");
                var rss = data.DeserializeXml<Rss>();
            }
         
        }
        [Test]
        public void Scrape_AnyState_ReturnsFeed()
        {
            ScraperConfig config = new ScraperConfig
            {
                UserName = _configuration.AppSettings["UserName"],
                Password = _configuration.AppSettings["Password"],
                Url = "https://www.freelancer.com/dashboard",
                ApiUrl = "https://www.freelancer.com/ajax/notify/live-feed/pre-populated.php"
            };

            bool result = true;
            using (NewsFeedScraper scraper = new NewsFeedScraper(null))
            {
                try
                {
                    scraper.GoToDashboard(config);
                    Feed feed = scraper.TryScrapeFeed(config);

                    if (feed != null && feed.Items != null)
                    {
                        foreach (var item in feed.Items)
                        {
                            _logger.Trace($"{item.Id} - {item.JobString} - {item.Title} - {item.Url} - {item.Description}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    // take screenshot
                    scraper.TakeScreenshot();
                    scraper.SaveSource();
                    result = false;
                }
            }

            result.Should().BeTrue();
        }
    }
}

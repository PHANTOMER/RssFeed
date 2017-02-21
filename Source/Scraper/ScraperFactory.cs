namespace Core.Scraper
{
    class ScraperFactory
    {
        public NewsFeedScraper Create(ScraperConfig config)
        {
            return new NewsFeedScraper(config.Proxies);
        }
    }
}

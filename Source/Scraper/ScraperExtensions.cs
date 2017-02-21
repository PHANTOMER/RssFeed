using OpenQA.Selenium;

namespace Core.Scraper
{
    public static class ScraperExtensions
    {
        public static IJavaScriptExecutor Scripts(this IWebDriver driver)
        {
            return (IJavaScriptExecutor) driver;
        }
    }
}

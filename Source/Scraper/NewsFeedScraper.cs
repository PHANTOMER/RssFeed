using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Common;
using Core.Extensions;
using Core.Models;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Cookie = System.Net.Cookie;

namespace Core.Scraper
{
    class ScraperException : Exception
    {
        public ScraperException(string message, ScraperConfig config) : base(message)
        {
            Config = config;
        }

        public ScraperConfig Config { get; }

        public override string ToString()
        {
            return $"{Message}. Config: {Config}";
        }
    }

    public class ScraperConfig
    {
        public string Url { get; set; }

        public string ApiUrl { get; set; }

        public string LoginUrl { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string[] Proxies { get; set; }

        public override string ToString()
        {
            return $"Url: {Url}; LoginUrl: {LoginUrl}; UserName: {UserName}; Password: {Password}";
        }
    }

    class NewsFeedScraper : ScraperBase
    {
        public NewsFeedScraper(string[] proxies) : base(proxies)
        {
            
        }
        public void GoToDashboard(ScraperConfig config)
        {
            int maxRetries = 5;
            int retry = 0;
            while (retry < maxRetries)
            {
                GoToUrl(config.Url);
                if (Regex.IsMatch(WebDriver.Url, @"/login") || WebDriver.Url == config.Url)
                {
                    break;
                }

                retry++;
            }
            
            if (WebDriver.Url != config.Url)
            {
                Login(config);
            }
        }

        public Feed TryScrapeFeed(ScraperConfig config)
        {
            CookieContainer cookieJar = new CookieContainer();
            var cookies = WebDriver.Manage().Cookies.AllCookies;
            foreach (var cookie in cookies)
            {
                cookieJar.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }

            int maxRetries = 5;
            int retry = 0;

            Logger.Trace("Downloading {0}", config.ApiUrl);

            Feed feed = null;

            while (true)
            {
                if (retry >= maxRetries)
                    throw new ScraperException("Can't download project feed. Retries exceeded", config);

                try
                {
                    using (var client = new CookieWebClient(cookieJar))
                    {
                        if (!ProxyAddress.IsNullOrEmpty())
                        {
                            client.Proxy = new WebProxy(ProxyAddress);
                        }

                        var mozilaAgent =
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36";
                        client.Headers.Add("User-Agent", mozilaAgent);

                        // Call authenticated resources
                        string result = client.DownloadString(config.ApiUrl);

                        feed = JsonConvert.DeserializeObject<Feed>(result);
                    }

                    break;
                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to download {0} page. Retry {1} of {2}", config.ApiUrl, retry, maxRetries);
                    Logger.Error(e);
                    retry++;
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                }
            }

            Logger.Info("Successfully downloaded {0}", config.ApiUrl);

            return feed;
        }

        public void Login(ScraperConfig config)
        {
            var userNameInput = FindElementWaitSafe(By.CssSelector("#username"), 1);
            var passwordInput = FindElementWaitSafe(By.CssSelector("#passwd"), 1);
            var loginBtn = FindElementWaitSafe(By.CssSelector("#login_btn"), 1);
            
            if (userNameInput == null || passwordInput == null || loginBtn == null)
                throw new ScraperException("Invalid login page", config);

            userNameInput.Clear();
            passwordInput.Clear();

            userNameInput.SendKeys(config.UserName);
            passwordInput.SendKeys(config.Password);

            loginBtn.Click();
        }
    }
}

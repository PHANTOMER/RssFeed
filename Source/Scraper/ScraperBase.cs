using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;

namespace Core.Scraper
{
    abstract class ScraperBase : IDisposable
    {
        protected readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private bool _disposed;

        protected string ProxyAddress;

        protected ScraperBase(string[] proxies)
        {
            if (proxies != null)
            {
                Random rand = new Random(DateTime.UtcNow.Millisecond);
                string address = proxies[rand.Next(0, proxies.Length)];
                ProxyAddress = address;
                Proxy proxy = new Proxy {HttpProxy = address};
                var service = PhantomJSDriverService.CreateDefaultService();
                service.ProxyType = "http";
                service.Proxy = proxy.HttpProxy;
                WebDriver = new PhantomJSDriver(service);
            }
            else
            {
                WebDriver = new PhantomJSDriver();
            }
        }

        public IWebDriver WebDriver { get; private set; }

        #region private util methods


        public IWebElement FindElementWait(By by, int waitSec = 10)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(waitSec));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            IWebElement element = wait.Until<IWebElement>((d) => d.FindElement(@by));
            return element;
        }

        public IWebElement FindElementWait(IWebElement parent, By by, int waitSec = 10)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(waitSec));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            IWebElement element = wait.Until<IWebElement>((d) => parent.FindElement(@by));
            return element;
        }

        public IEnumerable<IWebElement> FindElementsWait(By by, int waitSec = 10)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(waitSec));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            return wait.Until<IEnumerable<IWebElement>>((d) => d.FindElements(@by));
        }

        public IWebElement FindElementWaitSafe(By by, int waitSec = 10)
        {
            try
            {
                return FindElementWait(by, waitSec);
            }
            catch { return null; }
        }

        public IWebElement FindElementWaitSafe(IWebElement parent, By by, int waitSec = 10)
        {
            try
            {
                return FindElementWait(parent, by, waitSec);
            }
            catch { return null; }
        }

        public IEnumerable<IWebElement> FindElementsWaitSafe(By by, int waitSec = 10)
        {
            try
            {
                return FindElementsWait(by, waitSec);
            }
            catch { return null; }
        }

        public IWebElement FindVisibleElementWait(By by, int waitSec = 10)
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(waitSec));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(by));
            return element;
        }

        public IWebElement FindVisibleElementWaitSafe(By by, int waitSec = 10)
        {
            try
            {
                return FindVisibleElementWait(by, waitSec);
            }
            catch { return null; }
        }
        

        protected bool CheckElementNotVisibleWait(By by, int waitSec = 10)
        {
            var timeout = DateTime.Now.AddSeconds(waitSec);
            bool isOk = false;
            try
            {
                while (!isOk)
                {
                    if (timeout < DateTime.Now)
                    {
                        throw new TimeoutException("Timeout");
                    }

                    IWebElement element = null;
                    try
                    {
                        element = ExpectedConditions.ElementIsVisible(by).Invoke(WebDriver);
                    }
                    catch { }

                    isOk = element == null;
                    if (isOk)
                        break;

                    // wait
                    Thread.Sleep(1000);
                }
            }
            catch (TimeoutException e) { }

            return isOk;
        }

        #endregion

        #region helper wrapping methods

        public void GoToUrl(string url)
        {
            Logger.Trace("Navigating to url: {0}", url);
            WebDriver.Navigate().GoToUrl(url);
        }

        public void TakeScreenshot()
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot) WebDriver).GetScreenshot();

                string screenshot = ss.AsBase64EncodedString;
                byte[] screenshotAsByteArray = ss.AsByteArray;
                var fullFilePath = GetScreenshotFullPath();
                ss.SaveAsFile(fullFilePath, ImageFormat.Jpeg); //use any of the built in image formating
                ss.ToString(); //same as string screenshot = ss.AsBase64EncodedString;
                Logger.Info("Took screenshot. File: {0}", fullFilePath);
            }
            catch (Exception exc)
            {
                Logger.Error("Failed to take screenshot. Error: {0}. Stack: {1}", exc.Message, exc.StackTrace);
            }
        }

        public void SaveSource()
        {
            try
            {
                var fullFilePath = GetPageSourceFullPath();
                File.WriteAllText(fullFilePath, WebDriver.PageSource);
                Logger.Info("Saved page source. File: {0}", fullFilePath);
            }
            catch (Exception exc)
            {
                Logger.Error("Failed to take screenshot. Error: {0}. Stack: {1}", exc.Message, exc.StackTrace);
            }
        }

        public string GetPageSource()
        {
            return WebDriver.PageSource;
        }

        private string GetScreenshotFullPath()
        {
            string fileName = Guid.NewGuid() + ".jpg";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, fileName);
        }

        private string GetPageSourceFullPath()
        {
            string fileName = Guid.NewGuid() + ".txt";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "page_sources");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, fileName);
        }

        #endregion      

        #region disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    WebDriver.Dispose();
                }

                // shared cleanup logic
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Disposed { get { return _disposed; } }

        #endregion
    }
}

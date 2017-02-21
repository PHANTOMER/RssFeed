using System;
using System.ServiceProcess;
using Core.Jobs;
using Microsoft.Owin.Hosting;
using NLog;

namespace RssFeed.Service
{
    class RssService : ServiceBase, IServiceBase
    {
        private readonly string _baseAddress;
        private readonly JobLauncher _jobLauncher;
        private IDisposable _app;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public event EventHandler ServiceStopped;
        private const string LogTag = "Rss";

        public RssService(string serviceName, string baseAddress, JobLauncher jobLauncher)
        {
            _baseAddress = baseAddress;
            ServiceName = serviceName;

            _jobLauncher = jobLauncher;
        }

        #region private methods

        protected override void OnStart(string[] args)
        {
            _logger.Info("{0} - Service named '{1}' is starting...", LogTag, ServiceName);

            try
            {
                // start job launcher
                //_jobLauncher.Init();

                // Start OWIN host 
                _app = WebApp.Start<Startup>(_baseAddress);

                //using (WebApp.Start<Startup>(url: baseAddress);)
                //{
                //     //Create HttpCient and make a request to api/values 
                //    HttpClient client = new HttpClient();

                //    var response = client.GetAsync(baseAddress + "api/values").Result;

                //    Console.WriteLine(response);
                //    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                //} 
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                throw;
            }

            _logger.Info("{0} - Service named '{1}' started!", LogTag, ServiceName);
        }

        protected override void OnStop()
        {
            if (_app != null)
                _app.Dispose();

            //_jobLauncher.Stop();

            OnServiceStopped(EventArgs.Empty);
        }

        private void OnServiceStopped(EventArgs e)
        {
            EventHandler stopped = ServiceStopped;
            if (stopped != null) stopped(this, e);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Stop();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void Open()
        {
            OnStart(null);
        }

        public void Close()
        {
            OnStop();
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.DataAccess;
using Core.Extensions;
using Core.Models;

namespace RssFeed.Services
{
    public interface IRssFeedProvider
    {
        Task<string> GetRssFeed();
    }

    class RssFeedProvider : IRssFeedProvider
    {
        const string CTEST = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<rss version=""2.0"" >

<channel>
  <title>W3Schools Home Page</title>
  <link>http://www.w3schools.com</link>
  <description>Free web building tutorials</description>
  <item>
    <title>RSS Tutorial</title>
    <link>http://www.w3schools.com/xml/xml_rss.asp</link>
    <description>New RSS tutorial on W3Schools</description>
  </item>
  <item>
    <title>XML Tutorial</title>
    <link>http://www.w3schools.com/xml</link>
    <description>New XML tutorial on W3Schools</description>
  </item>
</channel>

</rss>";

        private readonly IFeedDataService _feedDataService;
        private readonly IConfigurationProvider _configuration;

        public RssFeedProvider(IFeedDataService feedDataService, IConfigurationProvider configuration)
        {
            _feedDataService = feedDataService;
            _configuration = configuration;
        }

        public async Task<string> GetRssFeed()
        {
            string[] categories = _configuration.AppSettings["JobCategories"].Split(';');
            Rss rss;
            using (var client = new WebClient())
            {
                string data = await client.DownloadStringTaskAsync("https://www.freelancer.com/rss.xml");
                rss = data.DeserializeXml<Rss>();
                if (rss != null && rss.Feed != null && rss.Feed.Items != null)
                {
                    rss.Feed.Items = rss.Feed.Items.Where(item => item.Categories.Any(c => categories.Any(cc => string.Compare(c, cc, StringComparison.OrdinalIgnoreCase) == 0))).ToArray();
                }
            }

            //Rss rss = await _feedDataService.GetFeedAsync();

            return await Task.FromResult(rss == null ? "" : rss.SerializeToXml(Encoding.UTF8));
        }
    }
}

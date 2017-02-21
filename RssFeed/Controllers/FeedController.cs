using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac.Integration.WebApi;
using RssFeed.Services;

namespace RssFeed.Controllers
{
    [AutofacControllerConfiguration]
    public class FeedController : ApiController
    {
        private readonly IRssFeedProvider _rssFeedProvider;

        public FeedController(IRssFeedProvider rssFeedProvider)
        {
            _rssFeedProvider = rssFeedProvider;
        }

        public async Task<HttpResponseMessage> Get()
        {
            string feed = await _rssFeedProvider.GetRssFeed();

            return new HttpResponseMessage()
            {
                Content = new StringContent(feed, Encoding.UTF8, "application/xml")
            };
        }
    }
}

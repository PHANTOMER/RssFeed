using System.Web.Configuration;
using Core.Common;

namespace RssFeed.Configuration
{
    public class WebConfigurationProvider : ConfigurationProvider
    {
        public WebConfigurationProvider()
            : base(WebConfigurationManager.OpenWebConfiguration(null))
        {

        }
    }
}

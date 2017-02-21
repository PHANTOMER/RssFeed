using System.Configuration;

namespace Core.Common
{
    class AppConfigurationProvider : ConfigurationProvider
    {
        public AppConfigurationProvider() : base(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None))
        {

        }

        public AppConfigurationProvider(Configuration configuration) : base(configuration)
        {

        }

        public AppConfigurationProvider(string path) : base(ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = path }, ConfigurationUserLevel.None))
        {
            
        }
    }
}

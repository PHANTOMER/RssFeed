using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Collections.Specialized;
using NLog;

namespace Core.Common
{
    public interface IConfigurationProvider
    {
        object GetSection(string sectionName);

        NameValueCollection AppSettings { get; }

        NameValueCollection ConnectionStrings { get; }

        void UpdateSettings(IDictionary<string, object> values);

        void UpdateConnectionStrings(IDictionary<string, string> values);
    }

    public abstract class ConfigurationProvider : IConfigurationProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected Configuration Configuration;

        protected ConfigurationProvider(Configuration configuration)
        {
            Configuration = configuration;
        }

        public virtual object GetSection(string sectionName)
        {
            return Configuration.GetSection(sectionName);
        }

        public virtual NameValueCollection AppSettings
        {
            get
            {
                NameValueCollection collection = new NameValueCollection();
                foreach (var key in Configuration.AppSettings.Settings.AllKeys)
                {
                    collection.Add(key, Configuration.AppSettings.Settings[key].Value);
                }

                return collection;
            }
        }

        public NameValueCollection ConnectionStrings
        {
            get
            {
                NameValueCollection connectionStrings = new NameValueCollection();
                foreach (var cstring in Configuration.ConnectionStrings.ConnectionStrings.OfType<ConnectionStringSettings>())
                {
                    connectionStrings.Add(cstring.Name, cstring.ConnectionString);
                }

                return connectionStrings;
            }
        }

        public void UpdateSettings(IDictionary<string, object> values)
        {
            try
            {
                var settings = Configuration.AppSettings.Settings;

                foreach (var key in values.Keys)
                {
                    if (settings[key] == null)
                    {
                        settings.Add(key, values[key].ToString());
                    }
                    else
                    {
                        settings[key].Value = values[key].ToString();
                    }
                }

                Configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(Configuration.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.Fatal("Error writing app settings");
                throw;
            }
        }

        public void UpdateConnectionStrings(IDictionary<string, string> values)
        {
            try
            {
                var settings = Configuration.ConnectionStrings.ConnectionStrings;

                foreach (var key in values.Keys)
                {
                    if (settings[key] == null)
                    {
                        settings.Add(new ConnectionStringSettings(key, values[key]));
                    }
                    else
                    {
                        settings.Remove(key);
                        settings.Add(new ConnectionStringSettings(key, values[key]));
                    }
                }

                Configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(Configuration.ConnectionStrings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.Fatal("Error writing app settings");
                throw;
            }
        }
    }

    public static class ConfigProviderExtensions
    {
        public static void UpdateSection(this ConfigurationSection section)
        {
            try
            {
                section.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception)
            {
            }
        }
    }
}

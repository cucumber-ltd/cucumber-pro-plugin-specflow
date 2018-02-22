using System;
using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public static class CucumberProResultsUrlBuilder
    {
        public static string BuildCucumberProUrl(Config config)
        {
            return BuildCucumberProUrl(config, config.GetString(ConfigKeys.CUCUMBERPRO_PROJECTNAME));
        }

        public static string BuildCucumberProUrl(Config config, string projectName)
        {
            var cucumberProUrl = GetCucumberProUrl(config);
            return $"{cucumberProUrl}tests/results/{projectName}";
        }

        private static String GetCucumberProUrl(Config config)
        {
            var cucumberProUrl = config.GetString(ConfigKeys.CUCUMBERPRO_URL);
            return !cucumberProUrl.EndsWith("/") ? cucumberProUrl + "/" : cucumberProUrl;
        }
    }
}

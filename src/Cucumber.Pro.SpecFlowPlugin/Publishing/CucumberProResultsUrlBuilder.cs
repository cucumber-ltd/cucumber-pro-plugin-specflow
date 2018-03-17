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
            return BuildCucumberProUrl(cucumberProUrl, projectName);
        }

        public static string BuildCucumberProUrl(string cucumberProUrl, string projectName)
        {
            return $"{cucumberProUrl}tests/results/{EncodeUriComponent(projectName)}";
        }

        private static string GetCucumberProUrl(Config config)
        {
            var cucumberProUrl = config.GetString(ConfigKeys.CUCUMBERPRO_URL);
            return !cucumberProUrl.EndsWith("/") ? cucumberProUrl + "/" : cucumberProUrl;
        }

        private static string EncodeUriComponent(string s) 
        {
            return Uri.EscapeDataString(s)
                            .Replace("%21", "!")
                            .Replace("%27", "'")
                            .Replace("%28", "(")
                            .Replace("%29", ")");

        }
    }
}

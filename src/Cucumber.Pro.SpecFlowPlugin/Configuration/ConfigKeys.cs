﻿namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public static class ConfigKeys
    {
        public const string CUCUMBERPRO_URL = "cucumberpro.url";
        public const string CUCUMBERPRO_TOKEN = "cucumberpro.token";
        public const string CUCUMBERPRO_CONNECTION_TIMEOUT = "cucumberpro.connection.timeout";
        public const string CUCUMBERPRO_ENVMASK = "cucumberpro.envmask";
        public const string CUCUMBERPRO_LOGGING = "cucumberpro.logging";
        public const string CUCUMBERPRO_LOGFILE = "cucumberpro.logfile";
        public const string CUCUMBERPRO_PROJECTNAME = "cucumberpro.projectname";
        public const string CUCUMBERPRO_PROFILE = "cucumberpro.profile";
        public const string CUCUMBERPRO_RESULTS_FILE = "cucumberpro.results.file";

        // for testing
        public const string CUCUMBERPRO_TESTING_FORCEPUBLISH = "cucumberpro.testing.forcepublish";
        public const string CUCUMBERPRO_TESTING_DRYRUN = "cucumberpro.testing.dryrun";

        // resolved settings
        public const string CUCUMBERPRO_GIT_REVISION = "cucumberpro.git.revision";
        public const string CUCUMBERPRO_GIT_BRANCH = "cucumberpro.git.branch";
        public const string CUCUMBERPRO_GIT_TAG = "cucumberpro.git.tag";
        public const string CUCUMBERPRO_GIT_REPOSITORYROOT = "cucumberpro.git.repositoryroot";

        public static Config CreateDefaultConfig()
        {
            var config = new Config();
            config.SetNull(CUCUMBERPRO_TOKEN);
            config.Set(CUCUMBERPRO_URL, "https://app.cucumber.pro/");
            config.Set(CUCUMBERPRO_CONNECTION_TIMEOUT, 5000);
            config.Set(CUCUMBERPRO_ENVMASK, "SECRET|KEY|TOKEN|PASSWORD|PWD");
            config.Set(CUCUMBERPRO_LOGGING, "WARN");

            config.SetNull(CUCUMBERPRO_PROJECTNAME);
            return config;
        }

        public static string GetEnvVarName(string key)
        {
            return key.Replace('.', '_').ToUpperInvariant();
        }
    }
}

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public static class ConfigKeys
    {
        public const string CUCUMBERPRO_URL = "cucumberpro.url";
        public const string CUCUMBERPRO_TOKEN = "cucumberpro.token";
        public const string CUCUMBERPRO_CONNECTION_IGNOREERROR = "cucumberpro.connection.ignoreerror";
        public const string CUCUMBERPRO_CONNECTION_TIMEOUT = "cucumberpro.connection.timeout";
        public const string CUCUMBERPRO_ENVMASK = "cucumberpro.envmask";
        public const string CUCUMBERPRO_LOGGING = "cucumberpro.logging";

        // Project name
        public const string CUCUMBERPRO_PROJECTNAME = "cucumberpro.projectname";
        // https://confluence.atlassian.com/bamboo/bamboo-variables-289277087.html
        public const string bamboo_planRepository_name = "bamboo_planRepository_name";
        // https://circleci.com/docs/2.0/env-vars/#circleci-environment-variable-descriptions
        public const string CIRCLE_PROJECT_REPONAME = "CIRCLE_PROJECT_REPONAME";
        // https://docs.travis-ci.com/user/environment-variables/#Default-Environment-Variables
        public const string TRAVIS_REPO_SLUG = "TRAVIS_REPO_SLUG";

        public static Config CreateDefaultConfig()
        {
            var config = new Config();
            config.SetNull(CUCUMBERPRO_TOKEN);
            config.Set(CUCUMBERPRO_URL, "https://app.cucumber.pro/");
            config.Set(CUCUMBERPRO_CONNECTION_IGNOREERROR, true);
            config.Set(CUCUMBERPRO_CONNECTION_TIMEOUT, 5000);
            config.Set(CUCUMBERPRO_ENVMASK, "SECRET|KEY|TOKEN|PASSWORD|PWD");
            config.Set(CUCUMBERPRO_LOGGING, "WARN");

            config.SetNull(CUCUMBERPRO_PROJECTNAME);
            config.SetNull(bamboo_planRepository_name);
            config.SetNull(CIRCLE_PROJECT_REPONAME);
            config.SetNull(TRAVIS_REPO_SLUG);
            return config;
        }
    }
}

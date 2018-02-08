using System.Collections.Generic;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.EnvironmentSettings
{
    public class EnvFilterTests
    {
        [Fact]
        public void Filters_and_sorts_keys()
        {
            var env = new Dictionary<string, string>
            {
                {"my_secret__token", "abcd"},
                {"MY_SECRET_TOKEN", "abcd"},
                {"A_KEY_TO_A_DOOR", "clef"},
                {"FOO", "BAR"},
                {"ALPHA", "BETA"},
                {"DOO", "dar"},
                {"PASSWORD_A", "drowssap"},
            };

            var config = ConfigKeys.CreateDefaultConfig();
            new EnvironmentVariablesConfigLoader(env).Load(config);
            EnvFilter envFilter = new EnvFilter(config);

            var expectedEnv = new Dictionary<string, string>
            {
                {"FOO", "BAR"},
                {"ALPHA", "BETA"},
                {"DOO", "dar"},
            };

            Assert.Equal(expectedEnv, envFilter.Filter(env));
        }

        [Fact]
        public void Allows_overriding_mask()
        {
            var env = new Dictionary<string, string>
            {
                {"CUCUMBERPRO_ENVMASK", "KEY|TOKEN"}, // But not SECRET|PASSWORD
                {"my_secret__token", "abcd"},
                {"MY_SECRET_TOKEN", "abcd"},
                {"A_KEY_TO_A_DOOR", "clef"},
                {"FOO", "BAR"},
                {"ALPHA", "BETA"},
                {"DOO", "dar"},
                {"PASSWORD_A", "drowssap"},
            };

            Config config = ConfigKeys.CreateDefaultConfig();
            new EnvironmentVariablesConfigLoader(env).Load(config);
            EnvFilter envFilter = new EnvFilter(config);

            var expectedEnv = new Dictionary<string, string>
            {
                {"CUCUMBERPRO_ENVMASK", "KEY|TOKEN"},
                {"FOO", "BAR"},
                {"ALPHA", "BETA"},
                {"DOO", "dar"},
                {"PASSWORD_A", "drowssap"},
            };
            Assert.Equal(expectedEnv, envFilter.Filter(env));
        }
    }
}

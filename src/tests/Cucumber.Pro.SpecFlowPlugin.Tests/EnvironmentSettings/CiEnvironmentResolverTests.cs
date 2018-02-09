using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.EnvironmentSettings
{
    public class CiEnvironmentResolverTests
    {
        private static Dictionary<string, string> GetJenkinsEnv()
        {
            return new Dictionary<string, string>
            {
                { "GIT_COMMIT", "rev1"},
                { "GIT_BRANCH", "branch1"}
            };
        }

        [Fact]
        public void Can_resolve_branch()
        {
            var resolver = CiEnvironmentResolver.Detect(GetJenkinsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("branch1", config.GetString(ConfigKeys.CUCUMBERPRO_BRANCH));
        }

        [Fact]
        public void Can_resolve_revision()
        {
            var resolver = CiEnvironmentResolver.Detect(GetJenkinsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("rev1", config.GetString(ConfigKeys.CUCUMBERPRO_REVISION));
        }

        [Fact]
        public void Configured_value_has_priority()
        {
            var resolver = CiEnvironmentResolver.Detect(GetJenkinsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_BRANCH, "overriden-branch");
            resolver.Resolve(config);

            Assert.Equal("overriden-branch", config.GetString(ConfigKeys.CUCUMBERPRO_BRANCH));
        }

        [Fact]
        public void Local_should_not_set_branch()
        {
            var resolver = CiEnvironmentResolver.Detect(new Dictionary<string, string>());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.True(config.IsNull(ConfigKeys.CUCUMBERPRO_BRANCH));
        }

        [Fact]
        public void Local_should_set_revision_to_timestamp()
        {
            var resolver = CiEnvironmentResolver.Detect(new Dictionary<string, string>());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.StartsWith("local20", config.GetString(ConfigKeys.CUCUMBERPRO_REVISION));
        }
    }
}

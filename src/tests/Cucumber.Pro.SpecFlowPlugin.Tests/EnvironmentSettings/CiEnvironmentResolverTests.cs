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
        public static Dictionary<string, string> GetJenkinsEnv()
        {
            return new Dictionary<string, string>
            {
                { "GIT_COMMIT", "rev1"},
                { "GIT_BRANCH", "branch1"}
            };
        }

        public static Dictionary<string, string> GetCircleEnv()
        {
            return new Dictionary<string, string>
            {
                { "CIRCLE_SHA1", "rev1"},
                { "CIRCLE_BRANCH", "branch1"},
                { "CIRCLE_PROJECT_REPONAME", "myproject"},
            };
        }

        public static Dictionary<string, string> GetTfsEnv()
        {
            return new Dictionary<string, string>
            {
                { "BUILD_SOURCEVERSION", "rev1"},
                { "BUILD_SOURCEBRANCHNAME", "branch1"},
                { "SYSTEM_TEAMPROJECT", "myproject"},
            };
        }

        public static Dictionary<string, string> GetTravisEnv()
        {
            return new Dictionary<string, string>
            {
                { "TRAVIS_COMMIT", "rev1"},
                { "TRAVIS_BRANCH", "branch1"},
                { "TRAVIS_REPO_SLUG", "owner/myproject"},
            };
        }

        [Fact]
        public void Can_resolve_branch()
        {
            var resolver = CiEnvironmentResolver.Detect(GetJenkinsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("branch1", config.GetString(ConfigKeys.CUCUMBERPRO_GIT_BRANCH));
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
        public void Can_resolve_project_name()
        {
            var resolver = CiEnvironmentResolver.Detect(GetCircleEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("myproject", config.GetString(ConfigKeys.CUCUMBERPRO_PROJECTNAME));
        }

        [Fact]
        public void Can_resolve_project_name_from_travis()
        {
            var resolver = CiEnvironmentResolver.Detect(GetTravisEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("myproject", config.GetString(ConfigKeys.CUCUMBERPRO_PROJECTNAME));
        }

        [Fact]
        public void Configured_value_has_priority()
        {
            var resolver = CiEnvironmentResolver.Detect(GetJenkinsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_GIT_BRANCH, "overriden-branch");
            resolver.Resolve(config);

            Assert.Equal("overriden-branch", config.GetString(ConfigKeys.CUCUMBERPRO_GIT_BRANCH));
        }

        [Fact]
        public void Local_should_not_set_branch()
        {
            var resolver = CiEnvironmentResolver.Detect(new Dictionary<string, string>());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.True(config.IsNull(ConfigKeys.CUCUMBERPRO_GIT_BRANCH));
        }

        [Fact]
        public void Local_should_set_revision_to_timestamp()
        {
            var resolver = CiEnvironmentResolver.Detect(new Dictionary<string, string>());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.StartsWith("local20", config.GetString(ConfigKeys.CUCUMBERPRO_REVISION));
        }

        [Fact]
        public void Can_resolve_config_from_tfs()
        {
            var resolver = CiEnvironmentResolver.Detect(GetTfsEnv());

            var config = ConfigKeys.CreateDefaultConfig();
            resolver.Resolve(config);

            Assert.Equal("myproject", config.GetString(ConfigKeys.CUCUMBERPRO_PROJECTNAME));
            Assert.Equal("rev1", config.GetString(ConfigKeys.CUCUMBERPRO_REVISION));
            Assert.Equal("branch1", config.GetString(ConfigKeys.CUCUMBERPRO_GIT_BRANCH));
        }

    }
}

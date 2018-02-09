using System.Linq;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.EnvironmentSettings
{
    public class EnvHelperTests
    {
        [Fact]
        public void Uses_case_insensitive_key_matching()
        {
            var env = EnvHelper.GetEnvironmentVariables();
            var upperCaseEnv = env.Keys.First(k => k.ToLowerInvariant() != k);

            Assert.Equal(env[upperCaseEnv], env[upperCaseEnv.ToLowerInvariant()]);
        }
    }
}

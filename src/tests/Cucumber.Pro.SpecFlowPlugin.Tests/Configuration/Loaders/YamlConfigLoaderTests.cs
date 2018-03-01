using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Configuration.Loaders
{
    public class YamlConfigLoaderTests : ConfigLoaderContract
    {
        protected override IConfigLoader MakeConfigLoader()
        {
            return new YamlConfigLoader(new StringReader(@"
cucumber:
  format: progress
"));
        }

        [Fact]
        public void removes_underscores_from_keys()
        {
            var config = new Config();
            var configLoader = new YamlConfigLoader(new StringReader(@"
cucumber:
  f_or_mat_: progress
"));
            configLoader.Load(config);

            Assert.Equal("progress", config.GetString("cucumber.format"));
        }

        [Fact]
        public void merges()
        {
            var config = new Config();
            new YamlConfigLoader(new StringReader("" +
                                                  "cucumberpro:\n" +
                                                  "  one: un\n")).Load(config);

            new YamlConfigLoader(new StringReader("" +
                                                  "cucumberpro:\n" +
                                                  "  two: deux\n")).Load(config);

            var yaml = "" +
                          "cucumberpro:\n" +
                          "  one: un\n" +
                          "  two: deux\n";
            Assert.Equal(yaml, config.ToString());

        }

        [Fact]
        public void roundtrips()
        {
            var yaml = "" +
                          "cucumberpro:\n" +
                          "  cucumberprofile: cucumber-jvm-unspecified-profile\n" +
                          "  envmask: SECRET|KEY|TOKEN|PASSWORD\n" +
                          "  logging: debug\n" +
                          "  url: https://app.cucumber.pro/\n" +
                          "  connection:\n" +
                          "    ignoreerror: true\n" +
                          "    timeout: 5000\n" +
                          "  git:\n" +
                          "    hostkey:\n" +
                          "    hostname: git.cucumber.pro\n" +
                          "    publish: false\n" +
                          "    sshport: 22\n" +
                          "    source:\n" +
                          "      fetch: true\n" +
                          "      remote: origin\n" +
                          "  project:\n" +
                          "    name:\n" +
                          "  results:\n" +
                          "    publish:\n" +
                          "    token:\n";

            var config = new Config();
            var configLoader = new YamlConfigLoader(new StringReader(yaml));
            configLoader.Load(config);

            Assert.Equal(yaml, config.ToString());
        }
    }
}

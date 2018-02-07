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
                                                  "cucumberpro:\r\n" +
                                                  "  one: un\r\n")).Load(config);

            new YamlConfigLoader(new StringReader("" +
                                                  "cucumberpro:\r\n" +
                                                  "  two: deux\r\n")).Load(config);

            var yaml = "" +
                          "cucumberpro:\r\n" +
                          "  one: un\r\n" +
                          "  two: deux\r\n";
            Assert.Equal(yaml, config.ToString());

        }

        [Fact]
        public void roundtrips()
        {
            var yaml = "" +
                          "cucumberpro:\r\n" +
                          "  cucumberprofile: cucumber-jvm-unspecified-profile\r\n" +
                          "  envmask: SECRET|KEY|TOKEN|PASSWORD\r\n" +
                          "  logging: debug\r\n" +
                          "  url: https://app.cucumber.pro/\r\n" +
                          "  connection:\r\n" +
                          "    ignoreerror: true\r\n" +
                          "    timeout: 5000\r\n" +
                          "  git:\r\n" +
                          "    hostkey:\r\n" +
                          "    hostname: git.cucumber.pro\r\n" +
                          "    publish: false\r\n" +
                          "    sshport: 22\r\n" +
                          "    source:\r\n" +
                          "      fetch: true\r\n" +
                          "      remote: origin\r\n" +
                          "  project:\r\n" +
                          "    name:\r\n" +
                          "  results:\r\n" +
                          "    publish:\r\n" +
                          "    token:\r\n";

            var config = new Config();
            var configLoader = new YamlConfigLoader(new StringReader(yaml));
            configLoader.Load(config);

            Assert.Equal(yaml, config.ToString());
        }
    }
}

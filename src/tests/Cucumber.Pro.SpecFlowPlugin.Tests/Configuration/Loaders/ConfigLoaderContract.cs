using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Configuration.Loaders
{
    public abstract class ConfigLoaderContract
    {
        [Fact]
        public void Creates_map_with_env()
        {
            var config = new Config();
            var configLoader = MakeConfigLoader();
            configLoader.Load(config);

            Assert.Equal("progress", config.GetString("cucumber.format"));
            Assert.Equal("progress", config.GetString("CUCUMBER_FORMAT"));
        }

        protected abstract IConfigLoader MakeConfigLoader();
    }
}

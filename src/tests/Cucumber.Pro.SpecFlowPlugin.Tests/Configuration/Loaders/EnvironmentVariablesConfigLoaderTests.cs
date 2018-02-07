using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Configuration.Loaders
{
    public class EnvironmentVariablesConfigLoaderTests : ConfigLoaderContract
    {
        protected override IConfigLoader MakeConfigLoader()
        {
            return new EnvironmentVariablesConfigLoader(new Dictionary<String, String>
            {
                { "CUCUMBER_FORMAT", "progress" }
            });
        }
    }
}

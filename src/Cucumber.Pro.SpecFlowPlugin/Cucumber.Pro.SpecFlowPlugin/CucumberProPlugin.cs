using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin;
using TechTalk.SpecFlow.Plugins;

[assembly: RuntimePlugin(typeof(CucumberProPlugin))]

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class CucumberProPlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters)
        {
        }
    }
}

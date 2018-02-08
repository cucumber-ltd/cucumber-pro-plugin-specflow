using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using TechTalk.SpecFlow.Plugins;

[assembly: RuntimePlugin(typeof(CucumberProPlugin))]

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class CucumberProPlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters)
        {
            runtimePluginEvents.RegisterGlobalDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterFactoryAs(ConfigFactory.Create);
            };

            runtimePluginEvents.ConfigurationDefaults += (sender, args) =>
            {
                args.SpecFlowConfiguration.AdditionalStepAssemblies.Add(
                    GetType().Assembly.FullName);
            };
        }
    }
}

using Cucumber.Pro.SpecFlowPlugin;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
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
                args.ObjectContainer.RegisterTypeAs<JsonReporter, IFormatter>("cpro");
            };

            runtimePluginEvents.ConfigurationDefaults += (sender, args) =>
            {
                args.SpecFlowConfiguration.AdditionalStepAssemblies.Add(
                    GetType().Assembly.FullName);
            };
        }
    }
}

using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class ScenarioStartedEvent : RuntimeEvent
    {
        public FeatureContext FeatureContext { get; }
        public ScenarioContext ScenarioContext { get; }

        public ScenarioStartedEvent(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            ScenarioContext = scenarioContext;
            FeatureContext = featureContext;
        }
    }
}

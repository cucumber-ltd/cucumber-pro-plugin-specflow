using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class ScenarioStartedEvent : RuntimeEvent
    {
        public ScenarioContext ScenarioContext { get; }

        public ScenarioStartedEvent(ScenarioContext scenarioContext)
        {
            ScenarioContext = scenarioContext;
        }
    }
}

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class StepFinishedEvent : RuntimeEvent
    {
        public ScenarioContext ScenarioContext { get; }
        public ScenarioStepContext StepContext => ScenarioContext.StepContext;

        public StepFinishedEvent(ScenarioContext scenarioContext)
        {
            ScenarioContext = scenarioContext;
        }
    }
}

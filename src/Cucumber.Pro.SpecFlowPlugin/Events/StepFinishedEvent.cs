using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class StepFinishedEvent : RuntimeEvent
    {
        public ScenarioContext ScenarioContext { get; }
        public ScenarioStepContext StepContext { get; }

        public StepFinishedEvent(ScenarioContext scenarioContext, ScenarioStepContext stepContext = null)
        {
            ScenarioContext = scenarioContext;
            StepContext = stepContext ?? scenarioContext.StepContext;
        }
    }
}

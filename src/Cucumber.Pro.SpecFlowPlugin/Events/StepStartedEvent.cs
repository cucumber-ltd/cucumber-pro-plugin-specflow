using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class StepStartedEvent : RuntimeEvent
    {
        public ScenarioContext ScenarioContext { get; }
        public ScenarioStepContext StepContext { get; }

        public StepStartedEvent(ScenarioContext scenarioContext, ScenarioStepContext stepContext = null)
        {
            ScenarioContext = scenarioContext;
            StepContext = stepContext ?? scenarioContext.StepContext;
        }
    }
}
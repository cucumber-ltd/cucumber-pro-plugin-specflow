using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class FeatureFinishedEvent : RuntimeEvent
    {
        public FeatureFinishedEvent(FeatureContext featureContext)
        {
            FeatureContext = featureContext;
        }

        public FeatureContext FeatureContext { get; }
    }
}

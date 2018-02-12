using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class ScenarioFinishedEvent : RuntimeEvent
    {
        public ScenarioContext ScenarioContext { get; }
        public FeatureContext FeatureContext { get; }

        public ScenarioFinishedEvent(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            ScenarioContext = scenarioContext;
            FeatureContext = featureContext;
        }
    }
}

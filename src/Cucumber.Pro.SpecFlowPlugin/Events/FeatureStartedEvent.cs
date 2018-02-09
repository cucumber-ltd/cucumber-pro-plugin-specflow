using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class FeatureStartedEvent : RuntimeEvent
    {
        public FeatureStartedEvent(FeatureContext featureContext)
        {
            FeatureContext = featureContext;
        }

        public FeatureContext FeatureContext { get; }
    }
}

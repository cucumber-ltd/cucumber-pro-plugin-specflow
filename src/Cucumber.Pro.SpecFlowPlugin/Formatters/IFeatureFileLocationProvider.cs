using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public interface IFeatureFileLocationProvider
    {
        string GetFeatureFilePath(FeatureContext featureContext);
        int? GetScenarioLine(ScenarioContext scenarioContext);
        int? GetStepLine(StepInstance stepInstance);
    }
}

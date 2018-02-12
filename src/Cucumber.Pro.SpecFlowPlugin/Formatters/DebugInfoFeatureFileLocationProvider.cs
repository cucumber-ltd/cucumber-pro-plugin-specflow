using System.Diagnostics;
using System.Linq;
using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public class DebugInfoFeatureFileLocationProvider : IFeatureFileLocationProvider
    {
        private StackFrame GetFeatureFileFrame()
        {
            var stackTrace = new StackTrace(true);
            var featureFileFrame = stackTrace.GetFrames()?.FirstOrDefault(
                f => f.GetFileName()?.EndsWith(".feature") ?? false);
            return featureFileFrame;
        }

        private int? GetCurrentFeatureFileLine() =>
            GetFeatureFileFrame()?.GetFileLineNumber();

        public string GetFeatureFilePath(FeatureContext featureContext) => GetFeatureFileFrame()?.GetFileName();
        public int? GetScenarioLine(ScenarioContext scenarioContext) => GetCurrentFeatureFileLine();
        public int? GetStepLine(ScenarioStepContext scenarioStepContext) => GetCurrentFeatureFileLine();
    }
}

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class FeatureResult
    {
        public int Line { get; set; } = 1;
        public string Name { get; set; } = "My Feature";
        public string Uri { get; set; } = "Features/EatingCucumbers.feature";

        [JsonProperty("elements")]
        public List<TestCaseResult> TestCaseResults { get; } = new List<TestCaseResult>();
    }
}

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class FeatureResult
    {
        //TODO: Line?
        public string Name { get; set; }
        public string Uri { get; set; }

        [JsonProperty("elements")]
        public List<TestCaseResult> TestCaseResults { get; } = new List<TestCaseResult>();
    }
}

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class TestCaseResult
    {
        public int Line { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        [JsonProperty("steps")]
        public List<StepResult> StepResults { get; } = new List<StepResult>();
    }
}

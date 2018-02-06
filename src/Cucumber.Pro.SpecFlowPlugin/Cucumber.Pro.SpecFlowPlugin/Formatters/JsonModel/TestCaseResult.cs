using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class TestCaseResult
    {
        public int Line { get; set; } = 9;
        public string Name = "Few cucumbers";
        public string Type = "Scenario";

        [JsonProperty("steps")]
        public List<StepResult> StepResults { get; } = new List<StepResult>();
    }
}

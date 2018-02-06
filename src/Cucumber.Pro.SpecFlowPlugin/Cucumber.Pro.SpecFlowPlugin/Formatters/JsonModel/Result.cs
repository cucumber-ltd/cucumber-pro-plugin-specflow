using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class Result
    {
        public long Duration { get; set; }
        public ResultStatus Status { get; set; }
    }
}

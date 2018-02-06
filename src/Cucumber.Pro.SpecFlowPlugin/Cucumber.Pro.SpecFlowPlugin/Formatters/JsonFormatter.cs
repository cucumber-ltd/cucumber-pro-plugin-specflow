using System.Collections.Generic;
using System.Linq;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public class JsonFormatter : IFormatter
    {
        private int _stepFinishedCount = 0;
        private readonly ITraceListener _traceListener;
        private readonly List<FeatureResult> _featureResults = new List<FeatureResult>();

        public JsonFormatter(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            publisher.RegisterHandlerFor(new RuntimeEventHandler<StepFinishedEvent>(OnStepFinished));

            //HACK: temporary hack
            var featureResult = new FeatureResult();
            featureResult.TestCaseResults.Add(new TestCaseResult());
            _featureResults.Add(featureResult);
        }

        private void OnStepFinished(StepFinishedEvent e)
        {
            var featureResult = _featureResults.Last();
            var testCaseResult = featureResult.TestCaseResults.Last();

            var stepResult = new StepResult()
            {
                Keyword = e.StepContext.StepInfo.StepInstance.Keyword,
                Name = e.StepContext.StepInfo.Text,
                Result = new Result()
                {
                    Duration = 1234, //TODO
                    Status = e.ScenarioContext.TestError == null ? ResultStatus.Passed : ResultStatus.Unknown
                }
            };
            testCaseResult.StepResults.Add(stepResult);

            _stepFinishedCount++;
            _traceListener.WriteToolOutput($"JsonFormatter: Step Finished ({_stepFinishedCount})");
        }

        public string GetJson()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.Formatting = Formatting.Indented;

            return JsonConvert.SerializeObject(_featureResults, serializerSettings);
        }
    }
}

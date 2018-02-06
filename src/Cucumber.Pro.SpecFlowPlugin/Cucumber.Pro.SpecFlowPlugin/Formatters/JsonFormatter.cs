using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public class JsonFormatter : IFormatter
    {
        private readonly ITraceListener _traceListener;
        private readonly List<FeatureResult> _featureResults = new List<FeatureResult>();

        public JsonFormatter(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            publisher.RegisterHandlerFor(new RuntimeEventHandler<ScenarioStartedEvent>(OnScenarioStarted));
            publisher.RegisterHandlerFor(new RuntimeEventHandler<StepFinishedEvent>(OnStepFinished));

            //HACK: temporary hack
            _featureResults.Add(new FeatureResult());
        }

        private void OnScenarioStarted(ScenarioStartedEvent e)
        {
            var featureResult = _featureResults.Last();

            var testCaseResult = new TestCaseResult
            {
                Line = GetFeatureFileLine(), 
                Name = e.ScenarioContext.ScenarioInfo.Title,
                Type = "scenario" //TODO
            };
            featureResult.TestCaseResults.Add(testCaseResult);
        }

        private static int GetFeatureFileLine()
        {
            var stackTrace = new StackTrace(true);
            var featureFileFrame = stackTrace.GetFrames()?.FirstOrDefault(
                f => f.GetFileName()?.EndsWith(".feature") ?? false);
            var line = featureFileFrame?.GetFileLineNumber() ?? 0;
            return line;
        }

        private void OnStepFinished(StepFinishedEvent e)
        {
            var featureResult = _featureResults.Last();
            var testCaseResult = featureResult.TestCaseResults.Last();

            var stepResult = new StepResult
            {
                Line = GetFeatureFileLine(),
                Keyword = e.StepContext.StepInfo.StepInstance.Keyword,
                Name = e.StepContext.StepInfo.Text,
                Result = new Result
                {
                    Duration = 0, //TODO
                    Status = e.ScenarioContext.TestError == null ? ResultStatus.Passed : ResultStatus.Failed,
                    ErrorMessage = e.ScenarioContext.TestError?.ToString()
                        //TODO: max length of error message
                }
            };
            testCaseResult.StepResults.Add(stepResult);

            _traceListener.WriteToolOutput($"JsonFormatter: Step Finished");
        }

        public string GetJson()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.Converters = new List<JsonConverter> {new StringEnumConverter {CamelCaseText = true}};
            serializerSettings.Formatting = Formatting.Indented;
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(_featureResults, serializerSettings);
        }
    }
}

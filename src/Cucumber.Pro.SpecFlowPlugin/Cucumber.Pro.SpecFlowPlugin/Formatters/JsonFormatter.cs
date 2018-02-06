using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private FeatureResult _currentFeatureResult = null;

        public JsonFormatter(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            publisher.RegisterHandlerFor(new RuntimeEventHandler<FeatureStartedEvent>(OnFeatureStarted));
            publisher.RegisterHandlerFor(new RuntimeEventHandler<ScenarioStartedEvent>(OnScenarioStarted));
            publisher.RegisterHandlerFor(new RuntimeEventHandler<StepFinishedEvent>(OnStepFinished));
        }

        private void OnFeatureStarted(FeatureStartedEvent e)
        {
            _currentFeatureResult = null; // this triggers feature initialization in OnScenarioStarted
        }

        private void OnScenarioStarted(ScenarioStartedEvent e)
        {
            if (_currentFeatureResult == null)
            {
                var featureFileFrame = GetFeatureFileFrame();
                var featureFile = featureFileFrame?.GetFileName() ?? "Unknown.feature";
                //TODO: make path relative to Git root

                _currentFeatureResult = new FeatureResult
                {
                    Uri = featureFile.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    Name = e.FeatureContext.FeatureInfo.Title
                };

                _featureResults.Add(_currentFeatureResult);
            }

            var testCaseResult = new TestCaseResult
            {
                Line = GetFeatureFileLine(), 
                Name = e.ScenarioContext.ScenarioInfo.Title,
                Type = "scenario" //TODO
            };
            _currentFeatureResult.TestCaseResults.Add(testCaseResult);
        }

        private static int GetFeatureFileLine()
        {
            var featureFileFrame = GetFeatureFileFrame();
            var line = featureFileFrame?.GetFileLineNumber() ?? 0;
            return line;
        }

        private static StackFrame GetFeatureFileFrame()
        {
            var stackTrace = new StackTrace(true);
            var featureFileFrame = stackTrace.GetFrames()?.FirstOrDefault(
                f => f.GetFileName()?.EndsWith(".feature") ?? false);
            return featureFileFrame;
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

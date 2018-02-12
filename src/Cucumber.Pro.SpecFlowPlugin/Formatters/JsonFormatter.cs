using System;
using System.Collections.Concurrent;
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
        private const string FEATURE_RESULT_KEY = "__JsonFormatter_FatureResult";
        private const string TESTCASE_RESULT_KEY = "__JsonFormatter_TestCaseResult";

        private readonly ITraceListener _traceListener;
        private readonly IDictionary<string, FeatureResult> _featureResultsById = new ConcurrentDictionary<string, FeatureResult>();
        private string _pathBaseFolder = null;

        internal IEnumerable<FeatureResult> FeatureResults => _featureResultsById.Values;

        public JsonFormatter(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public void SetPathBaseFolder(string pathBaseFolder)
        {
            if (FeatureResults.Any())
                throw new InvalidOperationException("The path base folder cannot be changed once there are features reported.");
            _pathBaseFolder = pathBaseFolder;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            publisher.RegisterHandlerFor(new RuntimeEventHandler<FeatureStartedEvent>(OnFeatureStarted));
            publisher.RegisterHandlerFor(new RuntimeEventHandler<ScenarioStartedEvent>(OnScenarioStarted));
            publisher.RegisterHandlerFor(new RuntimeEventHandler<StepFinishedEvent>(OnStepFinished));
        }

        private string GetFeatureKey(string featureFilePath, FeatureContext featureContext)
        {
            return featureFilePath ?? featureContext.FeatureInfo.Title;
        }

        private FeatureResult GetOrCreateFeatureResult(string key, Func<FeatureResult> createFeatureResult)
        {
            if (!_featureResultsById.TryGetValue(key, out var featureResult))
            {
                var newFeatureResult = createFeatureResult();

                lock (_featureResultsById)
                {
                    if (!_featureResultsById.TryGetValue(key, out featureResult))
                    {
                        _featureResultsById[key] = newFeatureResult;
                        featureResult = newFeatureResult;
                    }
                }
            }
            return featureResult;
        }

        private void OnFeatureStarted(FeatureStartedEvent e)
        {
            var featureFilePath = GetFeatureFileFrame()?.GetFileName();
            var featureKey = GetFeatureKey(featureFilePath, e.FeatureContext);
            var featureResult = GetOrCreateFeatureResult(featureKey, () =>
            {
                if (featureFilePath != null && Path.IsPathRooted(featureFilePath) && _pathBaseFolder != null)
                {
                    featureFilePath = PathHelper.MakeRelativePath(_pathBaseFolder, featureFilePath);
                }

                return new FeatureResult
                {
                    Uri = featureFilePath?.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    Name = e.FeatureContext.FeatureInfo.Title
                };
            });

            // the FeatureContext is exclusively used by the test thread
            e.FeatureContext[FEATURE_RESULT_KEY] = featureResult;
        }

        private void OnScenarioStarted(ScenarioStartedEvent e)
        {
            var featureResult = (FeatureResult)e.FeatureContext[FEATURE_RESULT_KEY];

            var testCaseResult = new TestCaseResult
            {
                Line = GetFeatureFileLine(), 
                Name = e.ScenarioContext.ScenarioInfo.Title,
                Type = "scenario" //TODO
            };

            // we need to protect the addition, because multiple test threads might run scenarios for the same feature file
            lock (featureResult)
            {
                featureResult.TestCaseResults.Add(testCaseResult);
            }

            // the ScenarioContext is exclusively used by the test thread
            e.ScenarioContext[TESTCASE_RESULT_KEY] = testCaseResult;
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
            var testCaseResult = (TestCaseResult)e.ScenarioContext[TESTCASE_RESULT_KEY];

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
        }

        public string GetJson()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.Converters = new List<JsonConverter> {new StringEnumConverter {CamelCaseText = true}};
            serializerSettings.Formatting = Formatting.Indented;
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;

            return JsonConvert.SerializeObject(FeatureResults, serializerSettings);
        }
    }
}

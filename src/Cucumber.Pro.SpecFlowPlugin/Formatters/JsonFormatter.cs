﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Infrastructure;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public class JsonFormatter : IFormatter
    {
        private const string FEATURE_RESULT_KEY = "__JsonFormatter_FatureResult";
        private const string TESTCASE_RESULT_KEY = "__JsonFormatter_TestCaseResult";

        private readonly IFeatureFileLocationProvider _featureFileLocationProvider;
        private readonly IDictionary<string, FeatureResult> _featureResultsById = new ConcurrentDictionary<string, FeatureResult>();
        private string _pathBaseFolder = null;

        internal IEnumerable<FeatureResult> FeatureResults => _featureResultsById.Values;

        public JsonFormatter(IFeatureFileLocationProvider featureFileLocationProvider)
        {
            _featureFileLocationProvider = featureFileLocationProvider;
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
            publisher.RegisterHandlerFor(new RuntimeEventHandler<ScenarioFinishedEvent>(OnScenarioFinished));
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
            var featureFilePath = _featureFileLocationProvider.GetFeatureFilePath(e.FeatureContext);
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
                Line = _featureFileLocationProvider.GetScenarioLine(e.ScenarioContext) ?? 0, 
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

        private ResultStatus GetResultStatus(ScenarioContext scenarioContext)
        {
            var testStatusProperty = scenarioContext.GetType().GetProperty("TestStatus", BindingFlags.Instance | BindingFlags.NonPublic);
            if (testStatusProperty == null)
                return scenarioContext.TestError == null ? ResultStatus.Passed : ResultStatus.Failed;

            switch ((TestStatus)testStatusProperty.GetValue(scenarioContext))
            {
                case TestStatus.TestError:
                    return ResultStatus.Failed;
                case TestStatus.BindingError:
                    return ResultStatus.Failed;
                case TestStatus.MissingStepDefinition:
                    return ResultStatus.Undefined;
                case TestStatus.StepDefinitionPending:
                    return ResultStatus.Pending;
                case TestStatus.OK:
                    return ResultStatus.Passed;
                default:
                    return ResultStatus.Unknown;
            }
        }

        private void OnStepFinished(StepFinishedEvent e)
        {
            var testCaseResult = (TestCaseResult)e.ScenarioContext[TESTCASE_RESULT_KEY];
            var stepLine = _featureFileLocationProvider.GetStepLine(e.StepContext.StepInfo.StepInstance);

            RegisterStepResult(testCaseResult, e.StepContext.StepInfo.StepInstance, e.ScenarioContext, stepLine);
        }

        private void RegisterStepResult(TestCaseResult testCaseResult, StepInstance stepInstance, ScenarioContext scenarioContext, int? stepLine)
        {
            var stepResult = new StepResult
            {
                Line = stepLine ?? 0,
                Keyword = stepInstance.Keyword,
                Name = stepInstance.Text,
                Result = new Result
                {
                    Duration = 0, //TODO
                    Status = GetResultStatus(scenarioContext),
                    ErrorMessage = scenarioContext.TestError?.ToString()
                }
            };
            testCaseResult.StepResults.Add(stepResult);
        }

        private void OnScenarioFinished(ScenarioFinishedEvent e)
        {
            var testCaseResult = (TestCaseResult)e.ScenarioContext[TESTCASE_RESULT_KEY];

            var status = GetResultStatus(e.ScenarioContext);
            if (status == ResultStatus.Undefined)
            {
                var missingStepsProperty = e.ScenarioContext.GetType().GetProperty("MissingSteps", BindingFlags.Instance | BindingFlags.NonPublic);
                if (missingStepsProperty != null)
                {
                    var missingSteps = (IEnumerable<StepInstance>)missingStepsProperty.GetValue(e.ScenarioContext);
                    foreach (var stepInstance in missingSteps)
                    {
                        var stepLine = _featureFileLocationProvider.GetStepLine(stepInstance);
                        RegisterStepResult(testCaseResult, stepInstance, e.ScenarioContext, stepLine);
                    }
                }
            }

            testCaseResult.Result = new Result
            {
                Duration = 0, //TODO
                Status = status,
                ErrorMessage = e.ScenarioContext.TestError?.ToString()
            };
        }

        public string GetJson(bool indented = false)
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

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
        private string _pathBaseFolder = null;

        public JsonFormatter(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public void SetPathBaseFolder(string pathBaseFolder)
        {
            if (_featureResults.Any())
                throw new InvalidOperationException("The path base folder cannot be changed once there are features reported.");
            _pathBaseFolder = pathBaseFolder;
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

                if (Path.IsPathRooted(featureFile) && _pathBaseFolder != null)
                {
                    featureFile = MakeRelativePath(_pathBaseFolder, featureFile);
                }

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

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            if (!fromPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                fromPath += Path.DirectorySeparatorChar;

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
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

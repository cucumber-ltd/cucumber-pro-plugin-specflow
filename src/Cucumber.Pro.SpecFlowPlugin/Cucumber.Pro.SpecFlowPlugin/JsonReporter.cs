using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class JsonReporter : IFormatter
    {
        private readonly IObjectContainer _objectContainer;
        private JsonFormatter _jsonFormatter;
        private IDictionary<string, string> _filteredEnv;
        private IResultsPublisher _resultsPublisher;

        public JsonReporter(IObjectContainer objectContainer)
        {
            // We cannot depend on the JsonFormatter directly, because it also implements IFormatter
            // and causes "Collection was modified" error when SpecFlow resolves all IFormatters.
            // So we delay the dependency resolution to the Initialize() method. This might be fixed in SpecFlow later.
            _objectContainer = objectContainer;
        }

        private void Initialize()
        {
            _jsonFormatter = _objectContainer.Resolve<JsonFormatter>();
            var envFilter = _objectContainer.Resolve<EnvFilter>();
            var config = _objectContainer.Resolve<Config>();
            var traceListener = _objectContainer.Resolve<ITraceListener>();

            //HACK:
            config.Set(ConfigKeys.CUCUMBERPRO_REVISION, "rev1");

            _filteredEnv = envFilter.Filter(EnvHelper.GetEnvironmentVariables());
            _resultsPublisher = ResultsPublisherFactory.Create(config, traceListener);
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            Initialize();

            _jsonFormatter.SetEventPublisher(publisher);

            publisher.RegisterHandlerFor<TestRunFinishedEvent>(OnTestRunFinished);
        }

        private void OnTestRunFinished(TestRunFinishedEvent e)
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            var path = Path.Combine(assemblyFolder, "result.json");
            File.WriteAllText(path, _jsonFormatter.GetJson());

            _resultsPublisher.PublishResults(path, _filteredEnv, "default");
        }
    }
}

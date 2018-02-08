using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class JsonReporter : IFormatter
    {
        private readonly IObjectContainer _objectContainer;
        private JsonFormatter _jsonFormatter;

        public JsonReporter(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            _jsonFormatter = _objectContainer.Resolve<JsonFormatter>();
            _jsonFormatter.SetEventPublisher(publisher);

            publisher.RegisterHandlerFor<TestRunFinishedEvent>(OnTestRunFinished);
        }

        private void OnTestRunFinished(TestRunFinishedEvent runtimeevent)
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            var path = Path.Combine(assemblyFolder, "result.json");
            File.WriteAllText(path, _jsonFormatter.GetJson());

            var publisher = new ResultsPublisher();
            publisher.PublishResults(path);
        }
    }
}

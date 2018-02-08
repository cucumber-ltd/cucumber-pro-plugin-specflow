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
        private readonly Lazy<JsonFormatter> _jsonFormatter;

        public JsonReporter(IObjectContainer objectContainer)
        {
            // We cannot depend on the JsonFormatter directly, because it also implements IFormatter
            // and causes "Collection was modified" error when SpecFlow resolves all IFormatters
            _jsonFormatter = new Lazy<JsonFormatter>(objectContainer.Resolve<JsonFormatter>, true);
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            _jsonFormatter.Value.SetEventPublisher(publisher);

            publisher.RegisterHandlerFor<TestRunFinishedEvent>(OnTestRunFinished);
        }

        private void OnTestRunFinished(TestRunFinishedEvent e)
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            var path = Path.Combine(assemblyFolder, "result.json");
            File.WriteAllText(path, _jsonFormatter.Value.GetJson());

            var publisher = new ResultsPublisher();
            publisher.PublishResults(path);
        }
    }
}

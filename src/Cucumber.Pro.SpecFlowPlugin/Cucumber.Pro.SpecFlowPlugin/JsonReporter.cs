using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class JsonReporter : IFormatter
    {
        private readonly JsonFormatter _jsonFormatter;

        public JsonReporter(JsonFormatter jsonFormatter)
        {
            _jsonFormatter = jsonFormatter;
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
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

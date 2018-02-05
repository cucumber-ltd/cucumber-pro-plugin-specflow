using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    [Binding]
    public class CucumberProPluginHooks
    {
        private static int _counter = 0;
        private readonly ITraceListener _traceListener;

        public CucumberProPluginHooks(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        [BeforeScenario]
        public void Test1()
        {
            _counter++;
            _traceListener.WriteToolOutput($"starting scenario #{_counter}");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine(_counter);
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            File.WriteAllText(
                Path.Combine(assemblyFolder, "result.json"), $"{_counter} scenarios started");
        }
    }
}

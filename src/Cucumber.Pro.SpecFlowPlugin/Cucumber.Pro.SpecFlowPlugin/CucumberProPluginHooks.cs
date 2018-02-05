using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Cucumber.Pro.SpecFlowPlugin
{
    [Binding]
    public class CucumberProPluginHooks
    {
        private static int _counter = 0;

        [BeforeScenario]
        public void Test1()
        {
            _counter++;
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

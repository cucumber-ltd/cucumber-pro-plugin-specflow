using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Tracing;
using Xunit.Abstractions;

namespace Cucumber.Pro.SpecFlowPlugin.SampleProject
{
    public class XUnitTraceListener : ITraceListener
    {
        private ITestOutputHelper GetTestOutputHelper()
        {
            ITestOutputHelper testOutputHelper = null;
            var scenarioContext = ScenarioContext.Current;
            if (scenarioContext != null && scenarioContext.ScenarioContainer.IsRegistered<ITestOutputHelper>())
            {
                testOutputHelper = scenarioContext.ScenarioContainer.Resolve<ITestOutputHelper>();
            }
            return testOutputHelper;
        }

        public void WriteTestOutput(string message)
        {
            var testOutputHelper = GetTestOutputHelper();
            testOutputHelper?.WriteLine(message);
        }

        public void WriteToolOutput(string message)
        {
            var testOutputHelper = GetTestOutputHelper();
            testOutputHelper?.WriteLine(">>> " + message);
        }
    }
}

using System.Collections.Generic;
using TechTalk.SpecFlow.Tracing;
using Xunit.Abstractions;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Publishing
{
    public class StubTraceListener : ITraceListener
    {
        private ITestOutputHelper _testOutputHelper;

        public StubTraceListener(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public List<string> TestOutput { get; } = new List<string>();
        public List<string> ToolOutput { get; } = new List<string>();

        public void WriteTestOutput(string message)
        {
            _testOutputHelper.WriteLine(message);
            TestOutput.Add(message);
        }

        public void WriteToolOutput(string message)
        {
            _testOutputHelper.WriteLine("> " + message);
            ToolOutput.Add(message);
        }
    }
}
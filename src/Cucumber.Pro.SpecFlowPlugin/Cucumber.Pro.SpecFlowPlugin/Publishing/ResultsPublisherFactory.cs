using Cucumber.Pro.SpecFlowPlugin.Configuration;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public static class ResultsPublisherFactory
    {
        public static IResultsPublisher Create(Config config, ITraceListener traceListener)
        {
            return new HttpMultipartResultsPublisher(config, traceListener);
        }
    }
}

using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public static class ResultsPublisherFactory
    {
        public static IResultsPublisher Create(Config config)
        {
            return new HttpMultipartResultsPublisher();
        }
    }
}

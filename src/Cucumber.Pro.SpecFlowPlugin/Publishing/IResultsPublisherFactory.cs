using Cucumber.Pro.SpecFlowPlugin.Configuration;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public interface IResultsPublisherFactory
    {
        IResultsPublisher Create(Config config, ILogger logger);
    }
}

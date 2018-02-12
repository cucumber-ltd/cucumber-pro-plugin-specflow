using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public interface IResultsPublisher
    {
        void PublishResultsFromContent(string resultsJson, IDictionary<string, string> env, string profileName);
    }
}

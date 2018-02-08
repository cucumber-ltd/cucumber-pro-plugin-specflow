using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public interface IResultsPublisher
    {
        void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName);
    }
}

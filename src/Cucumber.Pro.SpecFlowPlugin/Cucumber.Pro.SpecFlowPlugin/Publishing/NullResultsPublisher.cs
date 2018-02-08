using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class NullResultsPublisher : IResultsPublisher
    {
        public void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            //nop
        }
    }
}
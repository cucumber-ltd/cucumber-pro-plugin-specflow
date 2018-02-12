using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class NullResultsPublisher : IResultsPublisher
    {
        public void PublishResultsFromContent(string resultsJson, IDictionary<string, string> env, string profileName)
        {
            //nop
        }
    }
}

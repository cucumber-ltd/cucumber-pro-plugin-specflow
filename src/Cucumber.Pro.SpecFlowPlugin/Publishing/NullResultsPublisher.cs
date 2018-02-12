using System.Collections.Generic;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class NullResultsPublisher : IResultsPublisher
    {
        public void PublishResults(List<FeatureResult> resultsJson, IDictionary<string, string> env, string profileName)
        {
            //nop
        }
    }
}

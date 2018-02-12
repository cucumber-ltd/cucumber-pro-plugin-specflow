using System.Collections.Generic;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public interface IResultsPublisher
    {
        void PublishResultsFromContent(List<FeatureResult> resultsJson, IDictionary<string, string> env, string profileName);
    }
}

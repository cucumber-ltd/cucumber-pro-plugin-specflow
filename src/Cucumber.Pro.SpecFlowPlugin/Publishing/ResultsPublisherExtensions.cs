using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public static class ResultsPublisherExtensions
    {
        public static void PublishResults(this IResultsPublisher resultsPublisher, string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);
            resultsPublisher.PublishResultsFromContent(resultsJson, env, profileName);
        }

        public static void PublishResultsFromContent(this IResultsPublisher resultsPublisher, string resultsJson, IDictionary<string, string> env, string profileName)
        {
            var serializerSettings = JsonFormatter.GetJsonSerializerSettings(false);

            var featureResults = JsonConvert.DeserializeObject<List<FeatureResult>>(resultsJson, serializerSettings);
            resultsPublisher.PublishResults(featureResults, env, profileName);
        }
    }
}

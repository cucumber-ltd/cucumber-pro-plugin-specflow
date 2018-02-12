using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public static class ResultsPublisherExtensions
    {
        public static void PublishResults(this IResultsPublisher resultsPublisher, string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);
            resultsPublisher.PublishResultsFromContent(resultsJson, env, profileName);
        }
    }
}

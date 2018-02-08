using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class HttpMultipartResultsPublisher : IResultsPublisher
    {
        private const string PART_ENV = "env";
        private const string PART_PAYLOAD = "payload";
        private const string PART_PROFILE_NAME = "profileName";
        private const string CONTENT_TYPE_CUCUMBER_JAVA_RESULTS_JSON = "application/x.cucumber.java.results+json";
        private const string CONTENT_TYPE_SPECFLOW_RESULTS_JSON = CONTENT_TYPE_CUCUMBER_JAVA_RESULTS_JSON; // "application/x.specflow.results+json";

        private readonly string _url;

        public HttpMultipartResultsPublisher(string url = "https://app.cucumber.pro/tests/results/SpecSol_Test1/master")
        {
            _url = url;
        }

        public void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);
            PublishResultsFromContent(env, profileName, resultsJson);
        }

        private void PublishResultsFromContent(IDictionary<string, string> env, string profileName, string resultsJson)
        {
            var httpClient = new HttpClient();

            SetupAuthorization(httpClient);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(profileName), PART_PROFILE_NAME);
            content.Add(GetEnvContent(env), PART_ENV, "env.txt");
            content.Add(GetPayloadContent(resultsJson), PART_PAYLOAD, "results.json");

            var response = httpClient.PostAsync(_url, content).Result;
            Console.WriteLine(response);
        }

        private static StringContent GetPayloadContent(string resultsJson)
        {
            return new StringContent(resultsJson, Encoding.UTF8, CONTENT_TYPE_SPECFLOW_RESULTS_JSON);
        }

        private static StringContent GetEnvContent(IDictionary<string, string> env)
        {
            return new StringContent(string.Join("\n", env.Select(e => $"{e.Key}={e.Value}")));
        }

        private void SetupAuthorization(HttpClient httpClient)
        {
            var token = "fe3e1a5f27789a139a963ff56cddb00816c";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(token + ":")));
        }
    }
}

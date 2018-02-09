using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TechTalk.SpecFlow.Tracing;

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
        private readonly string _token;
        private readonly ITraceListener _traceListener;

        public HttpMultipartResultsPublisher(string url = "https://app.cucumber.pro/tests/results/SpecSol_Test1/master", string token = "fe3e1a5f27789a139a963ff56cddb00816c", ITraceListener traceListener = null)
        {
            _url = url;
            _token = token;
            _traceListener = traceListener;
        }

        public void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);
            PublishResultsFromContent(resultsJson, env, profileName);
        }

        private void PublishResultsFromContent(string resultsJson, IDictionary<string, string> env, string profileName)
        {
            var httpClient = new HttpClient();

            SetupAuthorization(httpClient);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(profileName), PART_PROFILE_NAME);
            content.Add(GetEnvContent(env), PART_ENV, "env.txt");
            content.Add(GetPayloadContent(resultsJson), PART_PAYLOAD, "payload.json");

            var response = httpClient.PostAsync(_url, content).Result;
            if (response.IsSuccessStatusCode)
                _traceListener?.WriteToolOutput($"Published results to Cucumber Pro: {_url}");

            Console.WriteLine(response);
        }

        private static StringContent GetPayloadContent(string resultsJson)
        {
            return new StringContent(resultsJson, Encoding.UTF8, CONTENT_TYPE_SPECFLOW_RESULTS_JSON);
        }

        private static StringContent GetEnvContent(IDictionary<string, string> env)
        {
            // CPro only supports Unix-like line endings (\n)
            return new StringContent(string.Join("\n", env.Select(e => $"{e.Key}={e.Value}")));
        }

        private void SetupAuthorization(HttpClient httpClient)
        {
            // CPro expects the token as the user name of the Basic auth. Basic auth requires
            // a Base64 encoded format of "username:password". In our case password is empty.
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_token + ":")));
        }
    }
}

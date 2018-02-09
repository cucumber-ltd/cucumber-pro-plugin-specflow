﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
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
        private readonly ILogger _logger;
        private readonly int _timeoutMilliseconds;

        public HttpMultipartResultsPublisher(Config config, ILogger logger) : this(
            logger,
            CucumberProResultsUrlBuilder.BuildCucumberProUrl(config),
            config.GetString(ConfigKeys.CUCUMBERPRO_TOKEN),
            config.GetInteger(ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT))
        {
        }

        public HttpMultipartResultsPublisher(ILogger logger, string url, string token, int timeoutMilliseconds)
        {
            _url = url;
            _token = token;
            _logger = logger;
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        public void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);
            PublishResultsFromContent(resultsJson, env, profileName);
        }

        private bool IsSupportedScheme(Uri uri)
        {
            return uri.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) ||
                   uri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);
        }

        private void PublishResultsFromContent(string resultsJson, IDictionary<string, string> env, string profileName)
        {
            try
            {
                if (!Uri.TryCreate(_url, UriKind.Absolute, out var urlUri) || !IsSupportedScheme(urlUri))
                {
                    _logger?.Log(TraceLevel.Error, $"Invalid URL for publising results to Cucumber Pro: {_url}");
                    return;
                }

                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMilliseconds(_timeoutMilliseconds);
                SetupAuthorization(httpClient);

                var content = new MultipartFormDataContent();
                content.Add(new StringContent(profileName), PART_PROFILE_NAME);
                content.Add(GetEnvContent(env), PART_ENV, "env.txt");
                content.Add(GetPayloadContent(resultsJson), PART_PAYLOAD, "payload.json");

                DumpDebugInfo(content);

                var response = httpClient.PostAsync(urlUri, content).Result;
                if (response.IsSuccessStatusCode)
                    _logger?.Log(TraceLevel.Info, $"Published results to Cucumber Pro: {_url}");
                else
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;

                    var suggestion = "";
                    if ((int) response.StatusCode == 401)
                        suggestion = $"You need to define {ConfigKeys.CUCUMBERPRO_TOKEN}";
                    else if ((int) response.StatusCode == 403)
                        suggestion = $"You need to change the value of {ConfigKeys.CUCUMBERPRO_TOKEN}";

                    var message =
                        $"Failed to publish results to Cucumber Pro URL: {_url}, Status: {(int) response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}{suggestion}{Environment.NewLine}{responseBody}";
                    _logger?.Log(TraceLevel.Error, message);
                }
            }
            catch (Exception timeoutEx) when(timeoutEx is TaskCanceledException ||
                (timeoutEx is AggregateException && timeoutEx.InnerException is TaskCanceledException))
            {
                _logger?.Log(TraceLevel.Warning, $"Publishing to Cucumber Pro timed out, consider increasing {ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT} (currently set to {_timeoutMilliseconds})");
            }
            catch (Exception ex)
            {
                _logger?.Log(TraceLevel.Error, "Unexpected error while publishing results to Cucumber Pro: " + ex);
            }
        }

        private void DumpDebugInfo(MultipartFormDataContent content)
        {
            if (_logger.Level < TraceLevel.Verbose)
                return;

            _logger.Log(TraceLevel.Verbose, $"CPro: Sending POST to {_url}");
            foreach (var part in content)
            {
                _logger.Log(TraceLevel.Verbose, $"PART: {part.Headers.ContentType}{Environment.NewLine}{part.ReadAsStringAsync().Result}");
            }
        }

        private static StringContent GetPayloadContent(string resultsJson)
        {
            return new StringContent(resultsJson, Encoding.UTF8, CONTENT_TYPE_SPECFLOW_RESULTS_JSON);
        }

        private static StringContent GetEnvContent(IDictionary<string, string> env)
        {
            // CPro only supports Unix-like line endings (\n)
            return new StringContent(string.Join("\n", env.OrderBy(e => e.Key).Select(e => $"{e.Key}={e.Value}")));
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Newtonsoft.Json;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class HttpSingleJsonResultsPublisher : IResultsPublisher
    {
        private const string CONTENT_TYPE_CUCUMBER_JAVA_RESULTS_JSON = "application/x.cucumber.java.results+json";
        private const string CONTENT_TYPE_SPECFLOW_RESULTS_JSON = CONTENT_TYPE_CUCUMBER_JAVA_RESULTS_JSON; // "application/x.specflow.results+json";

        private readonly string _url;
        private readonly string _token;
        private readonly ILogger _logger;
        private readonly int _timeoutMilliseconds;
        private readonly string _revision;
        private readonly string _branch;
        private readonly string _tag;

        public HttpSingleJsonResultsPublisher(Config config, ILogger logger) : this(
            logger,
            CucumberProResultsUrlBuilder.BuildCucumberProUrl(config),
            config.GetString(ConfigKeys.CUCUMBERPRO_TOKEN),
            config.GetInteger(ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT),
            config.GetString(ConfigKeys.CUCUMBERPRO_REVISION),
            config.IsNull(ConfigKeys.CUCUMBERPRO_GIT_BRANCH) ? null : config.GetString(ConfigKeys.CUCUMBERPRO_GIT_BRANCH),
            config.IsNull(ConfigKeys.CUCUMBERPRO_GIT_TAG) ? null : config.GetString(ConfigKeys.CUCUMBERPRO_GIT_TAG))
        {
        }

        public HttpSingleJsonResultsPublisher(ILogger logger, string url, string token, int timeoutMilliseconds, string revision, string branch, string tag)
        {
            _url = url;
            _token = token;
            _logger = logger;
            _timeoutMilliseconds = timeoutMilliseconds;
            _revision = revision;
            _branch = branch;
            _tag = tag;
        }

        private bool IsSupportedScheme(Uri uri)
        {
            return uri.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) ||
                   uri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);
        }

        class GitSettings
        {
            public string Revision { get; set; }
            public string Branch { get; set; }
            public string Tag { get; set; }
        }

        class ResultsPackage
        {
            public IDictionary<string, string> Environment { get; set; }
            public List<FeatureResult> CucumberJson { get; set; }
            public string ProfileName { get; set; }
            public GitSettings Git { get; set; }
        }

        private string GetSingleJsonContent(List<FeatureResult> featureResults, IDictionary<string, string> env, string profileName)
        {
            var resultsPackage = new ResultsPackage
            {
                Environment = env,
                CucumberJson = featureResults,
                ProfileName = profileName,
                Git = new GitSettings
                {
                    Branch = _branch,
                    Revision = _revision,
                    Tag = _tag
                }
            };
            var serializerSettings = JsonFormatter.GetJsonSerializerSettings(_logger.Level >= TraceLevel.Verbose);
            return JsonConvert.SerializeObject(resultsPackage, serializerSettings);
        }

        public void PublishResults(List<FeatureResult> resultsJson, IDictionary<string, string> env, string profileName)
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

                var content = new StringContent(GetSingleJsonContent(resultsJson, env, profileName),
                    Encoding.UTF8, CONTENT_TYPE_SPECFLOW_RESULTS_JSON);

                DumpDebugInfo(content);

                var response = httpClient.PostAsync(urlUri, content).Result;
                if (response.IsSuccessStatusCode)
                    _logger?.Log(TraceLevel.Info, $"Published results to Cucumber Pro: {_url}");
                else
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;

                    var suggestion = "";
                    if ((int)response.StatusCode == 401)
                        suggestion = $"You need to define {ConfigKeys.CUCUMBERPRO_TOKEN}";
                    else if ((int)response.StatusCode == 403)
                        suggestion = $"You need to change the value of {ConfigKeys.CUCUMBERPRO_TOKEN}";

                    var message =
                        $"Failed to publish results to Cucumber Pro URL: {_url}, Status: {(int)response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}{suggestion}{Environment.NewLine}{responseBody}";
                    _logger?.Log(TraceLevel.Error, message);
                }
            }
            catch (Exception timeoutEx) when (timeoutEx is TaskCanceledException ||
                (timeoutEx is AggregateException && timeoutEx.InnerException is TaskCanceledException))
            {
                _logger?.Log(TraceLevel.Warning, $"Publishing to Cucumber Pro timed out, consider increasing {ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT} (currently set to {_timeoutMilliseconds})");
            }
            catch (Exception ex)
            {
                _logger?.Log(TraceLevel.Error, "Unexpected error while publishing results to Cucumber Pro: " + ex);
            }
        }

        private void DumpDebugInfo(StringContent content)
        {
            if (_logger.Level < TraceLevel.Verbose)
                return;

            _logger.Log(TraceLevel.Verbose, $"CPro: Sending POST to {_url}");
            _logger.Log(TraceLevel.Verbose, $"PART: {content.Headers.ContentType}{Environment.NewLine}{content.ReadAsStringAsync().Result}");
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

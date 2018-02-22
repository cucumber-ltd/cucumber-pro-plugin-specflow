using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class JsonReporter : IFormatter
    {
        private const string DEFAULT_PROFILE = "default";

        private readonly IObjectContainer _objectContainer;
        private ILogger _logger;
        private JsonFormatter _jsonFormatter;
        private IDictionary<string, string> _envToSend;
        private IResultsPublisher _resultsPublisher;
        private string _profile;
        private bool _shouldPublish = false;

        public JsonReporter(IObjectContainer objectContainer)
        {
            // We cannot depend on the JsonFormatter directly, because it also implements IFormatter
            // and causes "Collection was modified" error when SpecFlow resolves all IFormatters.
            // So we delay the dependency resolution to the Initialize() method. This might be fixed in SpecFlow later.
            _objectContainer = objectContainer;
        }

        private void Initialize()
        {
            var jsonFormatter = _objectContainer.Resolve<JsonFormatter>();
            var envFilter = _objectContainer.Resolve<EnvFilter>();
            var config = _objectContainer.Resolve<Config>();
            var resultsPublisherFactory = _objectContainer.Resolve<ResultsPublisherFactory>();
            var environmentVariablesProvider = _objectContainer.Resolve<EnvironmentVariablesProvider>();
            var logger = _objectContainer.Resolve<Logger>();

            Initialize(config, envFilter, resultsPublisherFactory, jsonFormatter, environmentVariablesProvider, logger);
        }

        internal void Initialize(Config config, EnvFilter envFilter, IResultsPublisherFactory resultsPublisherFactory, JsonFormatter jsonFormatter, IEnvironmentVariablesProvider environmentVariablesProvider, ILogger logger)
        {
            _jsonFormatter = jsonFormatter;
            _logger = logger;
            _shouldPublish = true;

            var systemEnv = environmentVariablesProvider.GetEnvironmentVariables();
            DetectCiEnvironmentSettings(config, systemEnv, logger);

            if (!_shouldPublish)
                return;

            VerifyConfig(config, systemEnv);

            ConfigureGitRepositoryRoot(config, logger);
            ConfigureEnvToSend(config, envFilter, systemEnv);
            ConfigureProfile(config);

            _resultsPublisher = resultsPublisherFactory.Create(config, logger);
        }

        private void VerifyConfig(Config config, IDictionary<string, string> systemEnv)
        {
            if (config.IsNull(ConfigKeys.CUCUMBERPRO_PROJECTNAME))
                throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_PROJECTNAME} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_PROJECTNAME)}");
        }

        private void DetectCiEnvironmentSettings(Config config, IDictionary<string, string> systemEnv, ILogger logger)
        {
            var ciEnvironmentResolver = CiEnvironmentResolver.Detect(systemEnv);
            ciEnvironmentResolver.Resolve(config); // this sets revision, branch

            if (!ciEnvironmentResolver.IsDetected && !IsForcePublish(config))
            {
                _shouldPublish = false;
                logger.Log(TraceLevel.Info, $"Cucumber Pro plugin detected no CI environment and local publishing is not configured ({ConfigKeys.CUCUMBERPRO_RESULTS_PUBLISH}) -- skip publishing for this test run");
                return;
            }
            logger.Log(TraceLevel.Info, $"Cucumber Pro plugin detected CI environment as '{ciEnvironmentResolver.CiName}'");
        }

        private void ConfigureGitRepositoryRoot(Config config, ILogger logger)
        {
            if (!config.IsNull(ConfigKeys.CUCUMBERPRO_GIT_REPOSITORYROOT))
            {
                logger.Log(TraceLevel.Verbose,
                    $"CPro: Using '{config.GetString(ConfigKeys.CUCUMBERPRO_GIT_REPOSITORYROOT)}' as repository root.");
                _jsonFormatter.SetPathBaseFolder(config.GetString(ConfigKeys.CUCUMBERPRO_GIT_REPOSITORYROOT));
            }
        }

        private void ConfigureEnvToSend(Config config, EnvFilter envFilter, IDictionary<string, string> systemEnv)
        {
            _envToSend = envFilter.Filter(systemEnv);
        }

        private void ConfigureProfile(Config config)
        {
            _profile = config.IsNull(ConfigKeys.CUCUMBERPRO_PROFILE)
                ? DEFAULT_PROFILE
                : config.GetString(ConfigKeys.CUCUMBERPRO_PROFILE);
        }

        private static bool IsForcePublish(Config config)
        {
            return !config.IsNull(ConfigKeys.CUCUMBERPRO_RESULTS_PUBLISH) &&
                config.GetBoolean(ConfigKeys.CUCUMBERPRO_RESULTS_PUBLISH);
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            Initialize();

            if (_shouldPublish)
            {
                _jsonFormatter.SetEventPublisher(publisher);
                publisher.RegisterHandlerFor<TestRunFinishedEvent>(OnTestRunFinished);
            }
        }

        internal void OnTestRunFinished(TestRunFinishedEvent e)
        {
            if (!_shouldPublish)
                return;

            var featureResults = _jsonFormatter.FeatureResults.ToList();
            _resultsPublisher.PublishResults(featureResults, _envToSend, _profile);
        }
    }
}

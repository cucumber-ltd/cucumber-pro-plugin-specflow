using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private const string GIT_BRANCH_SEND = "GIT_BRANCH";

        private readonly IObjectContainer _objectContainer;
        private ILogger _logger;
        private JsonFormatter _jsonFormatter;
        private IDictionary<string, string> _envToSend;
        private IResultsPublisher _resultsPublisher;
        private string _profile;
        private string _resultsOutputFilePath;
        private bool _shouldPublish = false;

        internal string ResultsOutputFilePath => _resultsOutputFilePath;

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

            ConfigureResultsFile(config, logger);
            ConfigureGitRepositoryRoot(config, logger);
            ConfigureEnvToSend(config, envFilter, systemEnv);
            ConfigureProfile(config);

            _resultsPublisher = resultsPublisherFactory.Create(config, logger);
        }

        private void VerifyConfig(Config config, IDictionary<string, string> systemEnv)
        {
            if (config.IsNull(ConfigKeys.CUCUMBERPRO_PROJECTNAME))
                throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_PROJECTNAME} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_PROJECTNAME)}");
            if (!systemEnv.ContainsKey(GIT_BRANCH_SEND) && config.IsNull(ConfigKeys.CUCUMBERPRO_GIT_BRANCH))
                throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_GIT_BRANCH} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_GIT_BRANCH)}");
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

        private void ConfigureResultsFile(Config config, ILogger logger)
        {
            if (config.IsNull(ConfigKeys.CUCUMBERPRO_RESULTS_FILE))
                return;

            var fileName = Environment.ExpandEnvironmentVariables(config.GetString(ConfigKeys.CUCUMBERPRO_RESULTS_FILE));
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ??
                Directory.GetCurrentDirectory(); // in the very rare case the assembly folder cannot be detected, we use current directory

            _resultsOutputFilePath = Path.Combine(assemblyFolder, fileName);
            logger.Log(TraceLevel.Info, $"Saving Cucumber Pro results file to '{_resultsOutputFilePath}'.");
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
            if (!_envToSend.ContainsKey(GIT_BRANCH_SEND))
            {
                // we have verified already that it should be either in the system env or in the config
                _envToSend[GIT_BRANCH_SEND] = systemEnv.ContainsKey(GIT_BRANCH_SEND)
                    ? systemEnv[GIT_BRANCH_SEND]
                    : config.GetString(ConfigKeys.CUCUMBERPRO_GIT_BRANCH);
            }
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

            if (_resultsOutputFilePath != null)
            {
                var jsonContent = _jsonFormatter.GetJson(_logger.Level >= TraceLevel.Verbose);
                File.WriteAllText(_resultsOutputFilePath, jsonContent);
            }

            _resultsPublisher.PublishResults(featureResults, _envToSend, _profile);
        }
    }
}

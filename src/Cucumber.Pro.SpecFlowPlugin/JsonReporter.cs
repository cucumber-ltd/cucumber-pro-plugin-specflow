using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class JsonReporter : IFormatter
    {
        private const string DEFAULT_PROFILE = "default";
        private const string CUCUMBERPRO_GIT_BRANCH_SEND = "cucumber_pro_git_branch"; // should be ConfigKeys.CUCUMBERPRO_BRANCH

        private readonly IObjectContainer _objectContainer;
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

            var systemEnv = environmentVariablesProvider.GetEnvironmentVariables();
            var ciEnvironmentResolver = CiEnvironmentResolver.Detect(systemEnv);
            ciEnvironmentResolver.Resolve(config); // this sets revision, branch

            if (!ciEnvironmentResolver.IsDetected && !IsForcePublish(config))
            {
                _shouldPublish = false;
                logger.Log(TraceLevel.Info, $"Cucumber Pro plugin detected no CI environment and local publishing is not configured ({ConfigKeys.CUCUMBERPRO_RESULTS_PUBLISH}) -- skip publishing for this test run");
                return;
            }
            logger.Log(TraceLevel.Info, $"Cucumber Pro plugin detected CI environment as '{ciEnvironmentResolver.CiName}'");

            if (config.IsNull(ConfigKeys.CUCUMBERPRO_PROJECTNAME))
                throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_PROJECTNAME} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_PROJECTNAME)}");

            _envToSend = envFilter.Filter(systemEnv);
            if (!_envToSend.ContainsKey(CUCUMBERPRO_GIT_BRANCH_SEND))
            {
                if (config.IsNull(ConfigKeys.CUCUMBERPRO_BRANCH))
                    throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_BRANCH} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_BRANCH)}");
                _envToSend[CUCUMBERPRO_GIT_BRANCH_SEND] = config.GetString(ConfigKeys.CUCUMBERPRO_BRANCH);
            }

            _profile = config.IsNull(ConfigKeys.CUCUMBERPRO_PROFILE) ? DEFAULT_PROFILE :
                config.GetString(ConfigKeys.CUCUMBERPRO_PROFILE);

            _resultsPublisher = resultsPublisherFactory.Create(config, logger);
            _shouldPublish = true;
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

            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            var path = Path.Combine(assemblyFolder, "result.json");
            File.WriteAllText(path, _jsonFormatter.GetJson());

            _resultsPublisher.PublishResults(path, _envToSend, _profile);
        }
    }
}

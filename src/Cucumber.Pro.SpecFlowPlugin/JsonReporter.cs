﻿using System;
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
        private const string CUCUMBERPRO_GIT_BRANCH_SEND = "cucumber_pro_git_branch"; // should be ConfigKeys.CUCUMBERPRO_BRANCH

        private readonly IObjectContainer _objectContainer;
        private JsonFormatter _jsonFormatter;
        private IDictionary<string, string> _envToSend;
        private IResultsPublisher _resultsPublisher;

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
            var traceListener = _objectContainer.Resolve<ITraceListener>();
            var resultsPublisherFactory = _objectContainer.Resolve<ResultsPublisherFactory>();

            Initialize(config, envFilter, traceListener, resultsPublisherFactory, jsonFormatter);
        }

        internal void Initialize(Config config, EnvFilter envFilter, ITraceListener traceListener, IResultsPublisherFactory resultsPublisherFactory, JsonFormatter jsonFormatter)
        {
            _jsonFormatter = jsonFormatter;

            var systemEnv = EnvHelper.GetEnvironmentVariables();
            var ciEnvironmentResolver = CiEnvironmentResolver.Detect(systemEnv);
            ciEnvironmentResolver.Resolve(config); // this sets revision, branch

            if (config.IsNull(ConfigKeys.CUCUMBERPRO_PROJECTNAME))
                throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_PROJECTNAME} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_PROJECTNAME)}");

            _envToSend = envFilter.Filter(systemEnv);
            if (!_envToSend.ContainsKey(CUCUMBERPRO_GIT_BRANCH_SEND))
            {
                if (config.IsNull(ConfigKeys.CUCUMBERPRO_BRANCH))
                    throw new ConfigurationErrorsException($"Unable to detect git branch for publishing results to Cucumber Pro. Try to set the config value {ConfigKeys.CUCUMBERPRO_BRANCH} or the environment variable {ConfigKeys.GetEnvVarName(ConfigKeys.CUCUMBERPRO_BRANCH)}");
                _envToSend[CUCUMBERPRO_GIT_BRANCH_SEND] = config.GetString(ConfigKeys.CUCUMBERPRO_BRANCH);
            }
            _resultsPublisher = resultsPublisherFactory.Create(config, traceListener);
        }

        public void SetEventPublisher(IEventPublisher publisher)
        {
            Initialize();

            _jsonFormatter.SetEventPublisher(publisher);

            publisher.RegisterHandlerFor<TestRunFinishedEvent>(OnTestRunFinished);
        }

        internal void OnTestRunFinished(TestRunFinishedEvent e)
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            var path = Path.Combine(assemblyFolder, "result.json");
            File.WriteAllText(path, _jsonFormatter.GetJson());

            _resultsPublisher.PublishResults(path, _envToSend, "default");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Cucumber.Pro.SpecFlowPlugin.Tests.EnvironmentSettings;
using Moq;
using TechTalk.SpecFlow.Tracing;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests
{
    public class NullLogger : ILogger
    {
        public TraceLevel Level => TraceLevel.Off;

        public void Log(TraceLevel messageLevel, string message)
        {
            //nop
        }
    }

    public class JsonReporterTests
    {
        private Mock<IResultsPublisher> _resultsPublisherMock;
        private Dictionary<string, string> _env = new Dictionary<string, string>();

        private void InitializeReporter(JsonReporter reporter, Config config)
        {
            _resultsPublisherMock = new Mock<IResultsPublisher>();
            Mock<IResultsPublisherFactory> resultsPublisherFactoryStub = new Mock<IResultsPublisherFactory>();
            resultsPublisherFactoryStub.Setup(f => f.Create(It.IsAny<Config>(), It.IsAny<ILogger>()))
                .Returns(_resultsPublisherMock.Object);
            var traceListener = new NullListener();
            var environmentVariablesProviderMock = new Mock<IEnvironmentVariablesProvider>();
            environmentVariablesProviderMock.Setup(p => p.GetEnvironmentVariables())
                .Returns(_env);

            reporter.Initialize(config, new EnvFilter(config), resultsPublisherFactoryStub.Object,
                new JsonFormatter(new DebugInfoFeatureFileLocationProvider()), environmentVariablesProviderMock.Object, new NullLogger());
        }

        private static Config CreateUsualConfig()
        {
            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_GIT_BRANCH, "branch1");
            config.Set(ConfigKeys.CUCUMBERPRO_PROJECTNAME, "myproject");
            config.Set(ConfigKeys.CUCUMBERPRO_TESTING_FORCEPUBLISH, true);
            return config;
        }

        [Fact]
        public void Throws_when_projectname_cannot_be_detected()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.SetNull(ConfigKeys.CUCUMBERPRO_PROJECTNAME);

            Assert.Throws<ConfigurationErrorsException>(() =>
                InitializeReporter(reporter, config));
        }

        [Fact]
        public void Sets_profile_from_config()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_PROFILE, "myprofile");

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                It.IsAny<IDictionary<string, string>>(),
                "myprofile"));
        }

        [Fact]
        public void Uses_default_profile_if_not_configured()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.SetNull(ConfigKeys.CUCUMBERPRO_PROFILE);

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                It.IsAny<IDictionary<string, string>>(),
                "default"));
        }

        [Fact]
        public void Publishes_on_CI()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.SetNull(ConfigKeys.CUCUMBERPRO_TESTING_FORCEPUBLISH);
            _env = CiEnvironmentResolverTests.GetJenkinsEnv();

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<string>()));
        }

        [Fact]
        public void Publishes_on_local_when_configured()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            _env = CiEnvironmentResolverTests.GetConfiguredLocalEnv();

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<string>()));
        }

        [Fact]
        public void Publishes_on_local_when_forced()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_TESTING_FORCEPUBLISH, true);
            _env = new Dictionary<string, string>(); // simulate local

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<string>()));
        }

        [Fact]
        public void Does_not_publish_on_local()
        {
            var reporter = new JsonReporter(null);
            var config = CreateUsualConfig();
            config.SetNull(ConfigKeys.CUCUMBERPRO_TESTING_FORCEPUBLISH);
            _env = new Dictionary<string, string>(); // simulate local

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            _resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<List<FeatureResult>>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<string>()),
                Times.Never);
        }
    }
}

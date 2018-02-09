using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Moq;
using TechTalk.SpecFlow.Tracing;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests
{
    public class JsonReporterTests
    {
        private Mock<IResultsPublisher> resultsPublisherMock;

        private void InitializeReporter(JsonReporter reporter, Config config)
        {
            resultsPublisherMock = new Mock<IResultsPublisher>();
            Mock<IResultsPublisherFactory> resultsPublisherFactoryStub = new Mock<IResultsPublisherFactory>();
            resultsPublisherFactoryStub.Setup(f => f.Create(It.IsAny<Config>(), It.IsAny<ITraceListener>()))
                .Returns(resultsPublisherMock.Object);
            var traceListener = new NullListener();

            reporter.Initialize(config, new EnvFilter(config), traceListener, resultsPublisherFactoryStub.Object,
                new JsonFormatter(traceListener));
        }

        [Fact]
        public void Throws_when_branch_cannot_be_detected()
        {
            var reporter = new JsonReporter(null);
            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_PROJECTNAME, "myproject");

            Assert.Throws<ConfigurationErrorsException>(() =>
                InitializeReporter(reporter, config));
        }

        [Fact]
        public void Throws_when_projectname_cannot_be_detected()
        {
            var reporter = new JsonReporter(null);
            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_BRANCH, "branch1");

            Assert.Throws<ConfigurationErrorsException>(() =>
                InitializeReporter(reporter, config));
        }

        [Fact]
        public void Sets_cucumber_pro_git_branch()
        {
            var reporter = new JsonReporter(null);
            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_BRANCH, "branch1");
            config.Set(ConfigKeys.CUCUMBERPRO_PROJECTNAME, "myproject");

            InitializeReporter(reporter, config);
            reporter.OnTestRunFinished(new TestRunFinishedEvent());

            resultsPublisherMock.Verify(p =>
                p.PublishResults(It.IsAny<string>(),
                It.Is((IDictionary<string, string> env) => env.ContainsKey("cucumber_pro_git_branch")),
                It.IsAny<string>()));
        }
    }
}

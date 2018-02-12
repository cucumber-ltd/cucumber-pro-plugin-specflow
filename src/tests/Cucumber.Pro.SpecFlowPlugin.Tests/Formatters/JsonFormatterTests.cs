using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel;
using Moq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Tracing;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Formatters
{
    public class JsonFormatterTests
    {
        private bool _testRunStarted = false;

        private FeatureContext CreateFeatureContext(string name)
        {
            var container = new ObjectContainer();
            container.RegisterInstanceAs(new FeatureInfo(new CultureInfo("en-US"), name, null, ProgrammingLanguage.CSharp));
            container.RegisterInstanceAs(ConfigurationLoader.GetDefault());
            return container.Resolve<FeatureContext>();
        }

        private ScenarioContext CreateScenarioContext(string name)
        {
            var container = new ObjectContainer();
            container.RegisterInstanceAs(new ScenarioInfo(name));
            container.RegisterInstanceAs(new Mock<ITestObjectResolver>().Object);
            return container.Resolve<ScenarioContext>();
        }

        private ScenarioStepContext CreateStepContext(StepDefinitionType type, string text)
        {
            var container = new ObjectContainer();
            var stepInfo = new StepInfo(type, text, null, null);
            stepInfo.StepInstance = new StepInstance(type, (StepDefinitionKeyword)type, type + " ", text, null, null, null);
            container.RegisterInstanceAs(stepInfo);
            return container.Resolve<ScenarioStepContext>();
        }

        private void PublishScenarioExecution(EventPublisher eventPublisher, FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            PublishScenarioStart(eventPublisher, featureContext, scenarioContext);
            PublishScenarioFinish(eventPublisher, featureContext, scenarioContext);
        }

        private static void PublishScenarioFinish(EventPublisher eventPublisher, FeatureContext featureContext, ScenarioContext scenarioContext, bool testRunFinish = true)
        {
            //eventPublisher.Send(new ScenarioFinishedEvent());
            //eventPublisher.Send(new FeatureFinishedEvent());
            if (testRunFinish)
                eventPublisher.Send(new TestRunFinishedEvent());
        }

        private void PublishScenarioStart(EventPublisher eventPublisher, FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            if (!_testRunStarted)
            {
                eventPublisher.Send(new TestRunStartedEvent());
                _testRunStarted = true;
            }
            eventPublisher.Send(new FeatureStartedEvent(featureContext));
            eventPublisher.Send(new ScenarioStartedEvent(scenarioContext, featureContext));
        }

        private static void PublishStep(EventPublisher eventPublisher, ScenarioStepContext stepContext, ScenarioContext scenarioContext, Exception error = null)
        {
            //eventPublisher.Send(new StepStartedEvent());

            if (error != null)
            {
                SetTestError(scenarioContext, error);
            }

            eventPublisher.Send(new StepFinishedEvent(scenarioContext, stepContext));
        }

        private void PublishSampleStep(EventPublisher eventPublisher, ScenarioContext scenarioContext, StepDefinitionType type = StepDefinitionType.Given)
        {
            PublishStep(eventPublisher, CreateStepContext(type, "there is something"), scenarioContext);
        }

        private static void SetTestError(ScenarioContext scenarioContext, Exception error)
        {
            var testErrorProperty = scenarioContext.GetType().GetProperty(nameof(ScenarioContext.TestError));
            Assert.NotNull(testErrorProperty);
            testErrorProperty.SetValue(scenarioContext, error);
        }

        [Fact]
        public void Collects_feature_basics()
        {
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var featureContext = CreateFeatureContext("Feature1");
            var scenarioContext = CreateScenarioContext("Scenario1");

            PublishScenarioExecution(eventPublisher, featureContext, scenarioContext);

            var featureResult = Assert.Single(formatter.FeatureResults);
            Assert.Equal("Feature1", featureResult.Name);
        }

        [Fact]
        public void Collects_scenario_basics()
        {
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var featureContext = CreateFeatureContext("Feature1");
            var scenarioContext = CreateScenarioContext("Scenario1");

            PublishScenarioExecution(eventPublisher, featureContext, scenarioContext);

            var featureResult = Assert.Single(formatter.FeatureResults);
            var scenarioResult = Assert.Single(featureResult.TestCaseResults);
            Assert.Equal("Scenario1", scenarioResult.Name);
            Assert.Equal("scenario", scenarioResult.Type);
        }

        private static StepResult AssertStepResult(JsonFormatter formatter)
        {
            var featureResult = Assert.Single(formatter.FeatureResults);
            var scenarioResult = Assert.Single(featureResult.TestCaseResults);
            var stepResult = Assert.Single(scenarioResult.StepResults);
            return stepResult;
        }

        [Fact]
        public void Collects_step_basics()
        {
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var featureContext = CreateFeatureContext("Feature1");
            var scenarioContext = CreateScenarioContext("Scenario1");

            PublishScenarioStart(eventPublisher, featureContext, scenarioContext);
            eventPublisher.Send(new StepFinishedEvent(scenarioContext, CreateStepContext(StepDefinitionType.Given, "there is something")));
            PublishScenarioFinish(eventPublisher, featureContext, scenarioContext);

            var stepResult = AssertStepResult(formatter);
            Assert.Equal("there is something", stepResult.Name);
            Assert.Equal("Given ", stepResult.Keyword);
        }

        [Fact]
        public void Collects_successful_step_result()
        {
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var featureContext = CreateFeatureContext("Feature1");
            var scenarioContext = CreateScenarioContext("Scenario1");

            PublishScenarioStart(eventPublisher, featureContext, scenarioContext);
            PublishSampleStep(eventPublisher, scenarioContext);
            PublishScenarioFinish(eventPublisher, featureContext, scenarioContext);

            var stepResult = AssertStepResult(formatter);
            Assert.Equal(ResultStatus.Passed, stepResult.Result?.Status);
        }

        [Fact]
        public void Collects_failing_step_result()
        {
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var featureContext = CreateFeatureContext("Feature1");
            var scenarioContext = CreateScenarioContext("Scenario1");

            PublishScenarioStart(eventPublisher, featureContext, scenarioContext);
            var error = new Exception("simulated error");
            PublishStep(eventPublisher, CreateStepContext(StepDefinitionType.Given, "there is something"), scenarioContext, error);
            PublishScenarioFinish(eventPublisher, featureContext, scenarioContext);

            var stepResult = AssertStepResult(formatter);
            Assert.Equal(ResultStatus.Failed, stepResult.Result?.Status);
            Assert.StartsWith(error.ToString().Substring(0, 20), stepResult.Result?.ErrorMessage);
        }

        [Fact]
        public void Supports_parallel_execution()
        {
            // situation 2 features, 3 scenarios, they run parallel
            var formatter = new JsonFormatter(new NullListener());
            var eventPublisher = new EventPublisher();
            formatter.SetEventPublisher(eventPublisher);

            var feature1Context = CreateFeatureContext("Feature1");
            var feature2Context = CreateFeatureContext("Feature2");
            var scenario1Context = CreateScenarioContext("Scenario1");
            var scenario2Context = CreateScenarioContext("Scenario2");
            var scenario3Context = CreateScenarioContext("Scenario3");

            PublishScenarioStart(eventPublisher, feature1Context, scenario1Context);
            PublishScenarioStart(eventPublisher, feature1Context, scenario2Context);
            PublishScenarioStart(eventPublisher, feature2Context, scenario3Context);

            PublishSampleStep(eventPublisher, scenario3Context, StepDefinitionType.Then);
            PublishSampleStep(eventPublisher, scenario1Context, StepDefinitionType.Given);
            PublishSampleStep(eventPublisher, scenario2Context, StepDefinitionType.When);

            PublishScenarioFinish(eventPublisher, feature1Context, scenario2Context, false);
            PublishScenarioFinish(eventPublisher, feature2Context, scenario3Context, false);
            PublishScenarioFinish(eventPublisher, feature1Context, scenario1Context, true);

            var feature1Result = Assert.Single(formatter.FeatureResults.Where(fr => fr.Name == "Feature1"));
            var feature2Result = Assert.Single(formatter.FeatureResults.Where(fr => fr.Name == "Feature2"));

            var scenario1Result = Assert.Single(feature1Result.TestCaseResults.Where(tcr => tcr.Name == "Scenario1"));
            var scenario2Result = Assert.Single(feature1Result.TestCaseResults.Where(tcr => tcr.Name == "Scenario2"));
            var scenario3Result = Assert.Single(feature2Result.TestCaseResults.Where(tcr => tcr.Name == "Scenario3"));

            Assert.Single(scenario1Result.StepResults.Where(sr => sr.Keyword == "Given "));
            Assert.Single(scenario2Result.StepResults.Where(sr => sr.Keyword == "When "));
            Assert.Single(scenario3Result.StepResults.Where(sr => sr.Keyword == "Then "));
        }
    }
}

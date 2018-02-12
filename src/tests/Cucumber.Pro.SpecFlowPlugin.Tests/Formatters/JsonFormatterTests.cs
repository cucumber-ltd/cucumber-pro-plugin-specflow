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

        private static void PublishScenarioExecution(EventPublisher eventPublisher, FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            PublishScenarioStart(eventPublisher, featureContext, scenarioContext);
            PublishScenarioFinish(eventPublisher);
        }

        private static void PublishScenarioFinish(EventPublisher eventPublisher)
        {
            //eventPublisher.Send(new ScenarioFinishedEvent());
            //eventPublisher.Send(new FeatureFinishedEvent());
            eventPublisher.Send(new TestRunFinishedEvent());
        }

        private static void PublishScenarioStart(EventPublisher eventPublisher, FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            eventPublisher.Send(new TestRunStartedEvent());
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
            PublishScenarioFinish(eventPublisher);

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
            PublishStep(eventPublisher, CreateStepContext(StepDefinitionType.Given, "there is something"), scenarioContext);
            PublishScenarioFinish(eventPublisher);

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
            PublishScenarioFinish(eventPublisher);

            var stepResult = AssertStepResult(formatter);
            Assert.Equal(ResultStatus.Failed, stepResult.Result?.Status);
            Assert.StartsWith(error.ToString().Substring(0, 20), stepResult.Result?.ErrorMessage);
        }
    }
}

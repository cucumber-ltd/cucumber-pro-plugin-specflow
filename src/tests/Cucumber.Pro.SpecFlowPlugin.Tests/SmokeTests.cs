using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BoDi;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using Cucumber.Pro.SpecFlowPlugin.Tests.Publishing;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.Tracing;
using Xunit;
using Xunit.Abstractions;

namespace Cucumber.Pro.SpecFlowPlugin.Tests
{
    [Binding]
    public class SmokeTestStepDefinitions
    {
        private int _cucumbers = 0;

        [Given(@"I have already eaten (.*) cucumbers")]
        public void GivenIHaveAlreadyEatenCucumbers(int cucumbers)
        {
            _cucumbers = cucumbers;
        }

        [When(@"I eat (.*) cucumbers")]
        public void WhenIEatCucumbers(int cucumbers)
        {
            _cucumbers += cucumbers;
        }

        [Then(@"I should have (.*) cucumbers in my belly")]
        public void ThenIShouldHaveCucumbersInMyBelly(int expectedCount)
        {
            Assert.Equal(expectedCount, _cucumbers);
        }

    }

    public class SmokeTests
    {
        class StubFeatureFileLocationProvider : IFeatureFileLocationProvider
        {
            private int _stepLine;

            public string GetFeatureFilePath(FeatureContext featureContext)
            {
                return "src/tests/Cucumber.Pro.SpecFlowPlugin.SampleProject/Features/EatingCucumbers.feature";
            }

            public int? GetScenarioLine(ScenarioContext scenarioContext)
            {
                _stepLine = 10;
                return 10;
            }


            public int? GetStepLine(StepInstance stepInstance)
            {
                return ++_stepLine;
            }
        }

        class SmokeTestConfigurationProvider : IRuntimeConfigurationProvider
        {
            public SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration)
            {
                return specFlowConfiguration;
            }

            public IEnumerable<PluginDescriptor> GetPlugins(SpecFlowConfiguration specFlowConfiguration)
            {
                return new []
                {
                    new PluginDescriptor("Cucumber.Pro", null, PluginType.Runtime, null), 
                };
            }
        }

        class SmokeTestDependencyProvider : DefaultDependencyProvider
        {
            private readonly ITraceListener _traceListener;

            public SmokeTestDependencyProvider(StubTraceListener traceListener)
            {
                _traceListener = traceListener;
            }

            public override void RegisterGlobalContainerDefaults(ObjectContainer container)
            {
                base.RegisterGlobalContainerDefaults(container);
                container.RegisterInstanceAs<ITraceListener>(_traceListener);
            }
        }

        private readonly ITestOutputHelper _testOutputHelper;
        private StubTraceListener _smokeTestTraceListener;

        public SmokeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            Environment.SetEnvironmentVariable("CUCUMBERPRO_TESTING_FORCEPUBLISH", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CUCUMBERPRO_PROJECTNAME", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CUCUMBERPRO_GIT_BRANCH", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CUCUMBERPRO_TOKEN", null, EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable("GIT_COMMIT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("GIT_BRANCH", null, EnvironmentVariableTarget.Process);
        }

        private ITestRunner GetTestRunner()
        {
            _smokeTestTraceListener = new StubTraceListener(_testOutputHelper);
            var containerBuilder = new ContainerBuilder(new SmokeTestDependencyProvider(_smokeTestTraceListener));
            var smokeTestConfigurationProvider = new SmokeTestConfigurationProvider();
            var globalContainer = containerBuilder.CreateGlobalContainer(smokeTestConfigurationProvider);
            globalContainer.RegisterTypeAs<StubFeatureFileLocationProvider, IFeatureFileLocationProvider>();
            var testRunnerManager = globalContainer.Resolve<ITestRunnerManager>();
            testRunnerManager.Initialize(Assembly.GetExecutingAssembly());
            var testRunner = testRunnerManager.GetTestRunner(0);
            return testRunner;
        }

        [Fact]
        public void Publish_a_result_to_CPro_SaaS()
        {
            Environment.SetEnvironmentVariable("CUCUMBERPRO_PROJECTNAME", "SpecSol_Test1", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CUCUMBERPRO_TOKEN", "fe3e1a5f27789a139a963ff56cddb00816c", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CUCUMBERPRO_LOGGING", "debug", EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable("GIT_COMMIT", "sha", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("GIT_BRANCH", "master", EnvironmentVariableTarget.Process);

            var testRunner = GetTestRunner();
            RunScenario(testRunner);

            Assert.Contains(_smokeTestTraceListener.ToolOutput, m => m.Contains("Published results to Cucumber Pro"));
        }

        private static void RunScenario(ITestRunner testRunner)
        {
            var featureInfo = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Eating cucumbers", null,
                ProgrammingLanguage.CSharp, "smoke");
            testRunner.OnFeatureStart(featureInfo);

            var scenarioInfo = new ScenarioInfo("Few cucumbers", null);
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.Given("I have already eaten 5 cucumbers", null, null, "Given ");
            testRunner.When("I eat 2 cucumbers", null, null, "When ");
            testRunner.Then("I should have 7 cucumbers in my belly", null, null, "Then ");
            testRunner.CollectScenarioErrors();
            testRunner.OnScenarioEnd();

            testRunner.OnFeatureEnd();
            testRunner.OnTestRunEnd();
        }
    }
}

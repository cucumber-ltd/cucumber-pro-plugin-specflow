using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Cucumber.Pro.SpecFlowPlugin.Events;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    [Binding]
    public class CucumberProPluginHooks
    {
        private readonly ITraceListener _traceListener;
        private readonly EventPublisher _eventPublisher;
        private readonly IContextManager _contextManager;

        public CucumberProPluginHooks(ITraceListener traceListener, EventPublisher eventPublisher, IContextManager contextManager)
        {
            _traceListener = traceListener;
            _eventPublisher = eventPublisher;
            _contextManager = contextManager;
        }

        [BeforeTestRun]
        public static void SetupPlugin(JsonFormatter jsonFormatter, EventPublisher eventPublisher)
        {
            jsonFormatter.SetEventPublisher(eventPublisher);
        }

        [BeforeFeature]
        public static void BeforeFeature(EventPublisher eventPublisher, FeatureContext featureContext)
        {
            eventPublisher.Send(new FeatureStartedEvent(featureContext));
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _eventPublisher.Send(new ScenarioStartedEvent(_contextManager.ScenarioContext, _contextManager.FeatureContext));
        }

        [AfterStep]
        public void AfterStep()
        {
            _eventPublisher.Send(new StepFinishedEvent(_contextManager.ScenarioContext));
        }

        [AfterTestRun]
        public static void AfterTestRun(JsonFormatter jsonFormatter)
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            Debug.Assert(assemblyFolder != null);
            File.WriteAllText(
                Path.Combine(assemblyFolder, "result.json"), jsonFormatter.GetJson());
        }
    }
}

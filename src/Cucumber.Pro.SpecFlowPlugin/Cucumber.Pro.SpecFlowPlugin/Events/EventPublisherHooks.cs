using System.Collections.Generic;
using Cucumber.Pro.SpecFlowPlugin.Formatters;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    [Binding]
    public class EventPublisherHooks
    {
        private readonly EventPublisher _eventPublisher;
        private readonly IContextManager _contextManager;

        public EventPublisherHooks(EventPublisher eventPublisher, IContextManager contextManager)
        {
            _eventPublisher = eventPublisher;
            _contextManager = contextManager;
        }

        [BeforeTestRun]
        public static void SetupPlugin(IDictionary<string, IFormatter> formatters, EventPublisher eventPublisher)
        {
            foreach (var formatter in formatters.Values)
                formatter.SetEventPublisher(eventPublisher);

            eventPublisher.Send(new TestRunStartedEvent());
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
        public static void AfterTestRun(EventPublisher eventPublisher, JsonFormatter jsonFormatter)
        {
            eventPublisher.Send(new TestRunFinishedEvent());
        }
    }
}

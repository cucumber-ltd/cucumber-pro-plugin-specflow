using System;
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

        [BeforeTestRun(Order = Int32.MinValue)]
        public static void SetupPlugin(IDictionary<string, IFormatter> formatters, EventPublisher eventPublisher)
        {
            foreach (var formatter in formatters.Values)
                formatter.SetEventPublisher(eventPublisher);

            eventPublisher.Send(new TestRunStartedEvent());
        }

        [BeforeFeature(Order = Int32.MinValue)]
        public static void BeforeFeature(EventPublisher eventPublisher, FeatureContext featureContext)
        {
            eventPublisher.Send(new FeatureStartedEvent(featureContext));
        }

        [BeforeScenario(Order = Int32.MinValue)]
        public void BeforeScenario()
        {
            _eventPublisher.Send(new ScenarioStartedEvent(_contextManager.ScenarioContext, _contextManager.FeatureContext));
        }

        [Before(Order = Int32.MinValue)]
        public void BeforeStep()
        {
            _eventPublisher.Send(new StepStartedEvent(_contextManager.ScenarioContext));
        }

        [AfterStep(Order = Int32.MaxValue)]
        public void AfterStep()
        {
            _eventPublisher.Send(new StepFinishedEvent(_contextManager.ScenarioContext));
        }

        [AfterScenario(Order = Int32.MaxValue)]
        public void AfterScenario()
        {
            _eventPublisher.Send(new ScenarioFinishedEvent(_contextManager.ScenarioContext, _contextManager.FeatureContext));
        }

        [AfterFeature(Order = Int32.MaxValue)]
        public static void AfterFeature(EventPublisher eventPublisher, FeatureContext featureContext)
        {
            eventPublisher.Send(new FeatureFinishedEvent(featureContext));
        }

        [AfterTestRun(Order = Int32.MaxValue)]
        public static void AfterTestRun(EventPublisher eventPublisher, JsonFormatter jsonFormatter)
        {
            eventPublisher.Send(new TestRunFinishedEvent());
        }
    }
}

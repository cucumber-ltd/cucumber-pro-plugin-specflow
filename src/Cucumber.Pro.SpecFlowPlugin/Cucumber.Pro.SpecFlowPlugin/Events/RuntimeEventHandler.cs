namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public delegate void RuntimeEventHandler<in TEvent>(TEvent runtimeEvent) where TEvent : RuntimeEvent;
}

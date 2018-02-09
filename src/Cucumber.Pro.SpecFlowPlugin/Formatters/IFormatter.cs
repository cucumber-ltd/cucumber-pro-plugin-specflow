using Cucumber.Pro.SpecFlowPlugin.Events;

namespace Cucumber.Pro.SpecFlowPlugin.Formatters
{
    public interface IFormatter
    {
        void SetEventPublisher(IEventPublisher publisher);
    }
}
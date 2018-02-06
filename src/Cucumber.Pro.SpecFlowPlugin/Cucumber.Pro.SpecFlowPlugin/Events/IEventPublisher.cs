using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public interface IEventPublisher
    {
        void RegisterHandlerFor<TEvent>(RuntimeEventHandler<TEvent> handler) where TEvent : RuntimeEvent;
    }
}

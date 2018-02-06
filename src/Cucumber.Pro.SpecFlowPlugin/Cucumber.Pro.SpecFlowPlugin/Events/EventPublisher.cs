using System;
using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly Dictionary<Type, List<Delegate>> _registrations = new Dictionary<Type, List<Delegate>>();

        public void RegisterHandlerFor<TEvent>(RuntimeEventHandler<TEvent> handler) where TEvent : RuntimeEvent
        {
            //TODO: consider thread safety
            if (!_registrations.TryGetValue(typeof(TEvent), out var handlerList))
            {
                handlerList = new List<Delegate>();
                _registrations[typeof(TEvent)] = handlerList;
            }
            handlerList.Add(handler);
        }

        public void Send(RuntimeEvent runtimeEvent)
        {
            if (runtimeEvent == null) throw new ArgumentNullException(nameof(runtimeEvent));

            if (_registrations.TryGetValue(runtimeEvent.GetType(), out var handlerList))
            {
                foreach (var handler in handlerList)
                {
                    handler.DynamicInvoke(runtimeEvent);
                }
            }
        }
    }
}

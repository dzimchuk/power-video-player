using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class EventMessage : GenericMessage<Event>
    {
        private readonly EventArgs _args = EventArgs.Empty;

        public EventMessage(Event @event)
            : base(@event)
        {
        }

        public EventMessage(Event @event, EventArgs args)
            : base(@event)
        {
            _args = args;
        }

        public EventArgs EventArgs
        {
            get { return _args; }
        }
    }
}
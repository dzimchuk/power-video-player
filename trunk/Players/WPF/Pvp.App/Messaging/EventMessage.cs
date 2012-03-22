using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class EventMessage : GenericMessage<Event>
    {
        public EventMessage(Event @event)
            : base(@event)
        {
        }
    }
}

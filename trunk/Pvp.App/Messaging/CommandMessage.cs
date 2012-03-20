using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class CommandMessage : GenericMessage<Command>
    {
        public CommandMessage(Command command)
            : base(command)
        {
        }
    }
}

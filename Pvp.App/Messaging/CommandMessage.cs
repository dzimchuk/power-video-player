using System;
using System.Linq;
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
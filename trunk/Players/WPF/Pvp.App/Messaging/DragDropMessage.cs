using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class DragDropMessage : GenericMessage<string>
    {
        public DragDropMessage(string filename)
            : base(filename)
        {
        }
    }
}

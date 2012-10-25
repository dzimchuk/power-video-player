using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.Messaging
{
    internal class PlayNewFileMessage : GenericMessage<string>
    {
        public PlayNewFileMessage(string filename)
            : base(filename)
        {
        }
    }
}

using System;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    public class DestinationRectangleChangedEventArgs : EventArgs
    {
        private readonly GDI.RECT _newRect;

        public DestinationRectangleChangedEventArgs(GDI.RECT newRect)
        {
            _newRect = newRect;
        }

        public GDI.RECT NewDestinationRectangle
        {
            get { return _newRect; }
        }
    }
}
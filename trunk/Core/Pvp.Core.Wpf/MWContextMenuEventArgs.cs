using System;
using System.Linq;
using System.Windows;

namespace Pvp.Core.Wpf
{
    public class MWContextMenuEventArgs : RoutedEventArgs
    {
        private readonly Point _screenPosition;

        public MWContextMenuEventArgs(Point screenPosition)
        {
            _screenPosition = screenPosition;
        }

        public Point ScreenPosition
        {
            get { return _screenPosition; }
        }
    }
}

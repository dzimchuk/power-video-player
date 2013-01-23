using System;
using System.Linq;
using System.Windows;

namespace Pvp.Core.Wpf
{
    public class ScreenPositionEventArgs : RoutedEventArgs
    {
        private readonly Point _screenPosition;

        public ScreenPositionEventArgs(Point screenPosition)
        {
            _screenPosition = screenPosition;
        }

        public Point ScreenPosition
        {
            get { return _screenPosition; }
        }
    }
}

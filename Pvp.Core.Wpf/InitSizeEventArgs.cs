using System.Windows;

namespace Pvp.Core.Wpf
{
    public class InitSizeEventArgs : RoutedEventArgs
    {
        private readonly Size _newVideoSize;

        public InitSizeEventArgs(Size newVideoSize)
        {
            _newVideoSize = newVideoSize;
        }

        public Size NewVideoSize
        {
            get { return _newVideoSize; }
        }
    }
}
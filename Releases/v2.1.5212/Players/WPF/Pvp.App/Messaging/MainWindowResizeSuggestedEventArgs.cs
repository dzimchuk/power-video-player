using System;
using Pvp.Core.MediaEngine;

namespace Pvp.App.Messaging
{
    internal class MainWindowResizeSuggestedEventArgs : EventArgs
    {
        private readonly VideoSize _videoSize;
        private readonly double _width;
        private readonly double _height;

        public MainWindowResizeSuggestedEventArgs(VideoSize videoSize, double width, double height)
        {
            _videoSize = videoSize;
            _width = width;
            _height = height;
        }

        public VideoSize VideoSize
        {
            get { return _videoSize; }
        }

        public double Width
        {
            get { return _width; }
        }

        public double Height
        {
            get { return _height; }
        }
    }
}
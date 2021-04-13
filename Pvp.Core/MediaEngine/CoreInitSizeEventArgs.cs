using System;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    public class CoreInitSizeEventArgs : EventArgs
    {
        private readonly GDI.SIZE _newSize;
        private readonly double _nativeAspectRatio;

        private readonly bool _initial;
        private readonly bool _suggestInvalidate;

        public CoreInitSizeEventArgs(GDI.SIZE newSize, double nativeAspectRatio, bool initial, bool suggestInvalidate)
        {
            _newSize = newSize;
            _nativeAspectRatio = nativeAspectRatio;
            _initial = initial;
            _suggestInvalidate = suggestInvalidate;
        }

        public GDI.SIZE NewVideSize { get { return _newSize; } }
        public double NativeAspectRatio { get { return _nativeAspectRatio; } }
        public bool Initial { get { return _initial; } }
        public bool InvalidateSuggested { get { return _suggestInvalidate; } }
    }
}
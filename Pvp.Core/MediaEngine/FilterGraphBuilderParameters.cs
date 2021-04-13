using System;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine
{
    internal class FilterGraphBuilderParameters
    {
        public string Source;                           // regular
        public IntPtr MediaWindowHandle;                // regular and dvd
        public Renderer PreferredVideoRenderer;         // regular and dvd
            
        public string DiscPath;                         // dvd
        public AM_DVD_GRAPH_FLAGS Flags;                // dvd
        public Func<string, bool> OnPartialSuccessCallback; // dvd
    }
}
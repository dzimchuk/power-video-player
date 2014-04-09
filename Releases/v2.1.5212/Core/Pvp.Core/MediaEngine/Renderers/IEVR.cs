using System;
using System.Linq;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.Renderers
{
    public interface IEVR
    {
        IMFVideoDisplayControl MFVideoDisplayControl { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.Render
{
    public interface IEnhancedVideoRenderer
    {
        IMFVideoDisplayControl MFVideoDisplayControl { get; }
    }
}

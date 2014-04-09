using System;
using System.Linq;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.Renderers
{
    public interface IVMR9Windowless
    {
        IVMRWindowlessControl9 VMRWindowlessControl { get; }
    }
}

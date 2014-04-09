using System;
using System.Linq;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.Renderers
{
    public interface IVMRWindowless
    {
        IVMRWindowlessControl VMRWindowlessControl { get; }
    }
}

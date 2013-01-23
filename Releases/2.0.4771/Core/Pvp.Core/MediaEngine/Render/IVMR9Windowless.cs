using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.Render
{
    public interface IVMR9Windowless
    {
        IVMRWindowlessControl9 VMRWindowlessControl { get; }
    }
}

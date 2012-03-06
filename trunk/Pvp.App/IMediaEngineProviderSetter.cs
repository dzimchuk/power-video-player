using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dzimchuk.MediaEngine.Core;

namespace Dzimchuk.Pvp.App
{
    internal interface IMediaEngineProviderSetter
    {
        IMediaEngine MediaEngine { set; }
    }
}

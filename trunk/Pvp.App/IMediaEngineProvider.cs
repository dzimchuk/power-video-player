using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.Core.MediaEngine;

namespace Pvp.App
{
    internal interface IMediaEngineProvider
    {
        IMediaEngine MediaEngine { get; }
    }
}

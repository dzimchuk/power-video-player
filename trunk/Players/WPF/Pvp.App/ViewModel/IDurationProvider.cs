using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    internal interface IDurationProvider
    {
        TimeSpan Duration { get; }
    }
}

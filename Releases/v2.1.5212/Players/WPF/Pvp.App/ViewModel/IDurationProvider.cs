using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IDurationProvider
    {
        TimeSpan Duration { get; }
    }
}

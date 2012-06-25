using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    internal interface IDriveInfo
    {
        string Name { get; }
        string VolumeLabel { get; }
        bool IsReady { get; }
    }
}

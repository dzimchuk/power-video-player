using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IDriveInfo
    {
        string Name { get; }
        string VolumeLabel { get; }
        bool IsReady { get; }
    }
}

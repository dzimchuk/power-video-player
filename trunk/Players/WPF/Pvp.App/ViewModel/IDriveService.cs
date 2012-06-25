using System.Collections.Generic;

namespace Pvp.App.ViewModel
{
    internal interface IDriveService
    {
        IEnumerable<IDriveInfo> GetAvailableCDRomDrives();
    }
}
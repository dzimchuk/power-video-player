using System.Collections.Generic;

namespace Pvp.App.ViewModel
{
    public interface IDriveService
    {
        IEnumerable<IDriveInfo> GetAvailableCDRomDrives();
    }
}
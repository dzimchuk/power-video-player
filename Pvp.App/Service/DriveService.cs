using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class DriveService : IDriveService
    {
        public IEnumerable<IDriveInfo> GetAvailableCDRomDrives()
        {
            List<IDriveInfo> infos = new List<IDriveInfo>();

            var drives = System.IO.DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                try
                {
                	if (drive.DriveType == System.IO.DriveType.CDRom)
                        infos.Add(new DriveInfo(drive));
                }
                catch {}
            }

            return infos;
        }
    }
}

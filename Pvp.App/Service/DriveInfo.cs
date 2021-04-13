using System;
using System.Linq;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class DriveInfo : IDriveInfo
    {
        private readonly System.IO.DriveInfo _info;

        public DriveInfo(System.IO.DriveInfo info)
        {
            _info = info;
        }

        public string Name
        {
            get { return _info.Name; }
        }

        public string VolumeLabel
        {
            get { return _info.VolumeLabel; }
        }

        public bool IsReady
        {
            get { return _info.IsReady; }
        }
    }
}

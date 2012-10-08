using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    internal static class DefaultSettings
    {
        public static string SreenshotsFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
        }

        public static MouseWheelAction MouseWheekAction
        {
            get { return MouseWheelAction.Volume; }
        }
    }
}
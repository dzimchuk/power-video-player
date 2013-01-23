using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public static class FileTypes
    {
        public static string[] All
        {
            get
            {
                return new[]
                           {
                               "asf",
                               "avi",
                               "dat",
                               "divx",
                               "flv",
                               "ifo",
                               "m1v",
                               "m2v",
                               "mkv",
                               "mov",
                               "mp4",
                               "mpe",
                               "mpeg",
                               "mpg",
                               "qt",
                               "vob",
                               "wmv",
                               "3gp",
                               "3g2"
                           };
            }
        }
    }
}
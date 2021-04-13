using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.Core.MediaEngine
{
    public interface IMediaWindowHost
    {
        /// <summary>
        /// Returns the media window that will be used to paint video on.
        /// This method is called when the new video is rendered.
        /// </summary>
        /// <returns></returns>
        IMediaWindow GetMediaWindow();
    }
}

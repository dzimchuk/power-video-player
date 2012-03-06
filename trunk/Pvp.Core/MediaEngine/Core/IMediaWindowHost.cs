using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dzimchuk.MediaEngine.Core
{
    public interface IMediaWindowHost
    {
        /// <summary>
        /// Returns the media window that will be used to paint video on.
        /// This method is called when the new video is rendered.
        /// </summary>
        /// <returns></returns>
        IMediaWindow GetMediaWindow();

        /// <summary>
        /// Returns the handle to the media window host.
        /// The handle is used when the engine needs to calculate the media window's position
        /// relative to its host.
        /// </summary>
        IntPtr Handle { get; }
    }
}

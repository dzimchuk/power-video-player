/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dzimchuk.MediaEngine.Core
{
    public static class MediaEngineServiceProvider
    {
        /// <summary>
        /// Get a media engine implementation.
        /// </summary>
        /// <returns>Suitable media engine implementation.</returns>
        public static IMediaEngine GetMediaEngine(IMediaWindowHost mediaWindowHost)
        {
            ThreadPool.QueueUserWorkItem(RetrieveVideoRenderers);
            return new MediaEngine(mediaWindowHost);
        }

        private static IEnumerable<Renderer> _renderers = null;
        private static readonly object _syncRoot = new object();

        private static void RetrieveVideoRenderers(object state)
        {
            if (_renderers == null)
            {
                lock (_syncRoot)
                {
                    if (_renderers == null)
                    {
                        using (var manager = new MediaTypeManager())
                        {
                            _renderers = manager.GetPresentVideoRenderers();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Get a recommended renderer from Renderer enumeration for the current system.
        /// </summary>
        public static Renderer RecommendedRenderer
        {
            get
            {
                RetrieveVideoRenderers(null);

                Renderer r = Renderer.VR;
                OperatingSystem os = Environment.OSVersion;
                if (os.Platform == PlatformID.Win32NT || os.Platform == PlatformID.Win32S || os.Platform == PlatformID.Win32Windows || os.Platform == PlatformID.WinCE)
                {
                    if (os.Version.Major >= 6 && _renderers.Contains(Renderer.EVR))
                        r = Renderer.EVR;
                    else if (os.Version.Major == 5 && os.Version.Minor == 1 && _renderers.Contains(Renderer.VMR_Windowless))
                        r = Renderer.VMR_Windowless;
                }
                return r;
            }
        }

        /// <summary>
        /// Get a read-only list of all renderers that are supported on the current system.
        /// </summary>
        public static IEnumerable<Renderer> PresentRenderers
        {
            get
            {
                RetrieveVideoRenderers(null);
                return _renderers;
            }
        }
    }
}

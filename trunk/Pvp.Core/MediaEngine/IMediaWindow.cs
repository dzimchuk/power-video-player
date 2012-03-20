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
using Pvp.Core.DirectShow;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    /// <summary>
    /// Desribes a media window functionality.
    /// 
    /// Media window is the window that DirectShow renderers will paint video on.
    /// This window will be resized to the size of the video content (inspired by the odd EVR behavior).
    /// Thus, applications should host the media window inside another client window which is called
    /// Media Window Host. The host's client area defines the boundaries for the media engine.
    /// All calculations (video size, aspect ratio) are done relative to the host's client area.
    /// 
    /// Applications are responsible for painting the host's background.
    /// 
    /// New media window should be created for each video and passed to IMediaEngine.BuildGraph.
    /// The media engine will dispose the window when the graph is reset (if KeepOpen == FALSE).
    /// </summary>
    public interface IMediaWindow : IDisposable
    {
        /// <summary>
        /// Occurs when a message is received by the media window.
        /// This allows the engine to 'hook' into the window's message pump.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        /// <summary>
        /// Gets HWND of the media window.
        /// </summary>
        IntPtr Handle { get; }
        
        /// <summary>
        /// Invalidates the media window.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Re-positions the media window. The coordinates are given relative to the media window host.
        /// </summary>
        /// <param name="rcDest"></param>
        void Move(ref GDI.RECT rcDest);

        /// <summary>
        /// Sets control interfaces for windowless renderers. Only 1 interface will be set at a time,
        /// others will be set to null. If a windowed renderer is used all values will be set to null and
        /// the application won't have to do anything in response to WM_PAINT (windowed renderers create
        /// there own windows on top of the media window and will handle them).
        /// </summary>
        /// <param name="VMRWindowlessControl">
        /// VMR7 windowless control:
        /// call VMRWindowlessControl.RepaintVideo(_hwnd, hDC) in WM_PAINT;
        /// call VMRWindowlessControl.DisplayModeChanged() in WM_DISPLAYCHANGE.
        /// </param>
        /// <param name="VMRWindowlessControl9">
        /// VMR9 windowless control:
        /// call VMRWindowlessControl9.RepaintVideo(_hwnd, hDC) in WM_PAINT;
        /// call VMRWindowlessControl9.DisplayModeChanged() in WM_DISPLAYCHANGE.
        /// </param>
        /// <param name="MFVideoDisplayControl">
        /// EVR control:
        /// call MFVideoDisplayControl.RepaintVideo() in WM_PAINT.
        /// </param>
        void SetRendererInterfaces(IVMRWindowlessControl VMRWindowlessControl,
                                   IVMRWindowlessControl9 VMRWindowlessControl9,
                                   IMFVideoDisplayControl MFVideoDisplayControl);
    }
}

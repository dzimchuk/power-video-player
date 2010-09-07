﻿/* ****************************************************************************
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
using System.Text;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    /// <summary>
    /// Specifies the public contract of a video renderer.
    /// </summary>
    internal interface IRenderer
    {
        /// <summary>
        /// Gets the type of the video renderer represented by the current instance as a Renderer
        /// enumeration value.
        /// </summary>
        Renderer Renderer { get; }
        
        /// <summary>
        /// Closes the renderer and releases all interfaces associated with it.
        /// </summary>
        void Close();

        /// <summary>
        /// Sets the video position.
        /// </summary>
        /// <param name="rcSrc">Source rectangle relative to the native video size.</param>
        /// <param name="rcDest">Destination rectangle relative to the media window.</param>
        void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest);

        /// <summary>
        /// Gets the size and aspect ratio of the video that is being rendered.
        /// </summary>
        /// <param name="width">Receives the horizontal size.</param>
        /// <param name="height">Receives the vertical size.</param>
        /// <param name="ARWidth">Receives the horizontal aspect ratio value.</param>
        /// <param name="ARHeight">Receives the vertical aspect ration value.</param>
        void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight);
        
        /// <summary>
        /// Gets the unconnected input pin of the renderer. If the pin was connected before it gets unconnected.
        /// </summary>
        /// <returns>Unconnected input pin.</returns>
        IPin GetInputPin();

        /// <summary>
        /// Gets the image that is being shown by a renderer.
        /// </summary>
        /// <param name="header">Receives BITMAPINFOHEADER structure.</param>
        /// <param name="dibFull">
        /// Receives a pointer to the whole dib, that is often preceeded by BITMAPINFOHEADER.
        /// Caller must free the memory by calling Marshal.FreeCoTaskMem(dibFull);
        /// </param>
        /// <param name="dibDataOnly">
        /// Receives a pointer to the bitmap data excluding preceeding BITMAPINFOHEADER.
        /// Caller should NOT free the memory pointed to by dibDataOnly as it is actually inside 
        /// a memory block pointed by dibFull.
        /// </param>
        /// <returns>True upon success.</returns>
        bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly);

        /// <summary>
        /// Removes the renderer from the graph and releases all interfaces associated with it.
        /// </summary>
        void RemoveFromGraph();
    }
}

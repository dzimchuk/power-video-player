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

namespace Pvp.Core.MediaEngine
{
    /// <summary>
    /// Specifies the contract for the platform specific bitmap creator.
    /// </summary>
    public interface IImageCreator
    {
        /// <summary>
        /// Creates a bitmap.
        /// </summary>
        /// <param name="header">BITMAPINFOHEADER structure containing metadata.</param>
        /// <param name="bytes">Pointer to the bitmap data excluding BITMAPINFOHEADER</param>
        void CreateImage(ref BITMAPINFOHEADER header, IntPtr bytes);

        /// <summary>
        /// Saves an image.
        /// </summary>
        /// <param name="filename"></param>
        void Save(string filename);

        /// <summary>
        /// True if an image has been created.
        /// </summary>
        bool Created { get; }
    }
}

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
using System.Text;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal interface IRenderer
    {
        Renderer Renderer { get; }
        void Close();

        void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest);
        void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight);
        IPin GetInputPin();

        void RemoveFromGraph();
    }
}

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
using Dzimchuk.DirectShow;
using Dzimchuk.MediaEngine.Core.GraphBuilders;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal class VMRWindowed : WindowedRenderer
    {
        public VMRWindowed()
        {
            _renderer = Renderer.VMR_Windowed;
        }
        
        protected override Guid RendererID
        {
            get { return Clsid.VideoMixingRenderer; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            Trace.GetTrace().TraceWarning("Failed to instantiate VMR.");
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR to the graph
            int hr = pGraphBuilder.AddFilter(pBaseFilter, "VMR (Windowed)");
            errorFunc(hr, Error.AddVMR);
        }

        // VMR windowed behaves like a real windowed renderer, i.e. it must be configured after the video is rendered (compare with VMR9 windowed)
        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            base.Initialize(pGraphBuilder, hMediaWindow);

            IVMRAspectRatioControl pVMRAspectRatioControl = (IVMRAspectRatioControl)pBaseFilter; // will be released when we release IBaseFilter
            pVMRAspectRatioControl.SetAspectRatioMode(VMR_ASPECT_RATIO_MODE.VMR_ARMODE_NONE);
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig).GUID; }
        }
    }
}

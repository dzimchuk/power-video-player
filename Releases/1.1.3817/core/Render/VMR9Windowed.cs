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

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal class VMR9Windowed : WindowedRenderer
    {
        private const int NUMBER_OF_STREAMS = 1;
        
        public VMR9Windowed()
        {
            _renderer = Renderer.VMR9_Windowed;
        }
        
        protected override Guid RendererID
        {
            get { return Clsid.VideoMixingRenderer9; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            Trace.GetTrace().TraceWarning("Failed to instantiate VMR9.");
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR9 to the graph
            int hr = pGraphBuilder.AddFilter(pBaseFilter, "VMR9 (Windowed)");
            errorFunc(hr, Error.AddVMR9);
        }

        // we must set number of input pins to 1 and we need to do it before VMR9 is connected,
        // hence we need to do it in Initialize
        // but we won't call base's (WindowedRenderer) Initialize yet, it will be called _after_ the video stream is rendered as required by windowed renderers
        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        private bool _initialized = false;
        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            if (_initialized)
            {
                base.Initialize(pGraphBuilder, hMediaWindow);
            }
            else
            {
                IVMRFilterConfig9 pVMRFilterConfig9 = (IVMRFilterConfig9)pBaseFilter; // will be released when pBaseFilter is released
                pVMRFilterConfig9.SetNumberOfStreams(NUMBER_OF_STREAMS);

                IVMRAspectRatioControl9 pVMRAspectRatioControl9 = (IVMRAspectRatioControl9)pBaseFilter;
                pVMRAspectRatioControl9.SetAspectRatioMode(VMR9AspectRatioMode.VMR9ARMode_None);

                _initialized = true;
            }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig9).GUID; }
        }
    }
}

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

namespace Pvp.Core.MediaEngine.Renderers
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
            TraceSink.GetTraceSink().TraceWarning("Failed to instantiate VMR9.");
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR9 to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "VMR9 (Windowed)");
            errorFunc(hr, GraphBuilderError.AddVMR9);
        }

        // we must set number of input pins to 1 and we need to do it before VMR9 is connected,
        // hence we need to do it in Initialize
        // but we won't call base's (WindowedRenderer) Initialize yet, it will be called _after_ the video stream is rendered as required by windowed renderers
        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        private bool _initialized;
        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            if (_initialized)
            {
                base.Initialize(pGraphBuilder, hMediaWindow);
            }
            else
            {
                IVMRFilterConfig9 pVMRFilterConfig9 = (IVMRFilterConfig9)BaseFilter; // will be released when pBaseFilter is released
                pVMRFilterConfig9.SetNumberOfStreams(NUMBER_OF_STREAMS);

                IVMRAspectRatioControl9 pVMRAspectRatioControl9 = (IVMRAspectRatioControl9)BaseFilter;
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

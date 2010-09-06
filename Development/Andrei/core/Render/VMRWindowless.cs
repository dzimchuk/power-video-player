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
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal class VMRWindowless : RendererBase
    {
        private IVMRWindowlessControl pVMRWindowlessControl;
        private IVMRFilterConfig pVMRFilterConfig;
        
        public VMRWindowless()
        {
            _renderer = Renderer.VMR_Windowless;
        }
        
        public override void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest)
        {
            pVMRWindowlessControl.SetVideoPosition(/*ref rcSrc*/ IntPtr.Zero, ref rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight)
        {
            pVMRWindowlessControl.GetNativeVideoSize(out width, out height, out ARWidth, out ARHeight);
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR to the graph
            int hr = pGraphBuilder.AddFilter(pBaseFilter, "VMR (Windowless)");
            errorFunc(hr, Error.AddVMR);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the VMR interfaces
            try
            {
                pVMRFilterConfig = (IVMRFilterConfig)pBaseFilter;
                pVMRFilterConfig.SetRenderingMode(VMRMode.VMRMode_Windowless);
                pVMRWindowlessControl = (IVMRWindowlessControl)pBaseFilter;
                pVMRWindowlessControl.SetVideoClippingWindow(hMediaWindow);

                pVMRWindowlessControl.SetAspectRatioMode(VMR_ASPECT_RATIO_MODE.VMR_ARMODE_NONE);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.ConfigureVMR, e);
            }
        }

        protected override void CloseInterfaces()
        {
            pVMRFilterConfig = null;
            pVMRWindowlessControl = null; // they will be released when pBaseFilter is released

            base.CloseInterfaces(); // release pBaseFilter
        }

        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        protected override Guid RendererID
        {
            get { return Clsid.VideoMixingRenderer; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            Trace.GetTrace().TraceWarning("Failed to instantiate VMR.");
        }

        public IVMRWindowlessControl VMRWindowlessControl
        {
            get { return pVMRWindowlessControl; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig).GUID; }
        }
    }
}

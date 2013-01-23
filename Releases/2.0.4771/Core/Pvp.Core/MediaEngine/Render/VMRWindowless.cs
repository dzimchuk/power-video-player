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
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.GraphBuilders;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine.Render
{
    internal class VMRWindowless : RendererBase, IVMRWindowless
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

        public override void GetNativeVideoSize(out int width, out int height, out int arWidth, out int arHeight)
        {
            pVMRWindowlessControl.GetNativeVideoSize(out width, out height, out arWidth, out arHeight);
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "VMR (Windowless)");
            errorFunc(hr, Error.AddVMR);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the VMR interfaces
            try
            {
                pVMRFilterConfig = (IVMRFilterConfig)BaseFilter;
                pVMRFilterConfig.SetRenderingMode(VMRMode.VMRMode_Windowless);
                pVMRWindowlessControl = (IVMRWindowlessControl)BaseFilter;
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
            TraceSink.GetTraceSink().TraceWarning("Failed to instantiate VMR.");
        }

        public IVMRWindowlessControl VMRWindowlessControl
        {
            get { return pVMRWindowlessControl; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig).GUID; }
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int hr = pVMRWindowlessControl.GetCurrentImage(out dibFull);
            if (DsHlp.SUCCEEDED(hr))
            {
                header = (BITMAPINFOHEADER)Marshal.PtrToStructure(dibFull, typeof(BITMAPINFOHEADER));
                dibDataOnly = new IntPtr(dibFull.ToInt64() + Marshal.SizeOf(typeof(BITMAPINFOHEADER)));
                return true;
            }
            else
            {
                header = new BITMAPINFOHEADER();
                dibDataOnly = IntPtr.Zero;
                return false;
            }
        }
    }
}

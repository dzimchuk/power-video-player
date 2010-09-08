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
using Dzimchuk.DirectShow;
using Dzimchuk.MediaEngine.Core.GraphBuilders;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal class VMR9Windowless : RendererBase
    {
        private const int NUMBER_OF_STREAMS = 1;
        
        private IVMRWindowlessControl9 pVMRWindowlessControl9;
        private IVMRFilterConfig9 pVMRFilterConfig9;
        
        public VMR9Windowless()
        {
            _renderer = Renderer.VMR9_Windowless;
        }
        
        public override void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest)
        {
            pVMRWindowlessControl9.SetVideoPosition(/*ref rcSrc*/ IntPtr.Zero, ref rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight)
        {
            pVMRWindowlessControl9.GetNativeVideoSize(out width, out height, out ARWidth, out ARHeight);
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR9 to the graph
            int hr = pGraphBuilder.AddFilter(pBaseFilter, "VMR9 (Windowless)");
            errorFunc(hr, Error.AddVMR9);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the VMR9 interfaces
            try
            {
                pVMRFilterConfig9 = (IVMRFilterConfig9)pBaseFilter;
                pVMRFilterConfig9.SetRenderingMode(VMR9Mode.VMR9Mode_Windowless);
                pVMRFilterConfig9.SetNumberOfStreams(NUMBER_OF_STREAMS);
                pVMRWindowlessControl9 = (IVMRWindowlessControl9)pBaseFilter;
                pVMRWindowlessControl9.SetVideoClippingWindow(hMediaWindow);

                pVMRWindowlessControl9.SetAspectRatioMode(VMR9AspectRatioMode.VMR9ARMode_None);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.ConfigureVMR9, e);
            }
        }

        protected override void CloseInterfaces()
        {
            pVMRFilterConfig9 = null;
            pVMRWindowlessControl9 = null; // they will be released when pBaseFilter is released
            
            base.CloseInterfaces(); // release bBaseFilter
        }

        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        protected override Guid RendererID
        {
            get { return Clsid.VideoMixingRenderer9; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            Trace.GetTrace().TraceWarning("Failed to instantiate VMR9.");
        }

        public IVMRWindowlessControl9 VMRWindowlessControl
        {
            get { return pVMRWindowlessControl9; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig9).GUID; }
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int hr = pVMRWindowlessControl9.GetCurrentImage(out dibFull);
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

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
using Pvp.Core.MediaEngine.Internal;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine.Renderers
{
    internal class VMR9Windowless : RendererBase, IVMR9Windowless
    {
        private const int NUMBER_OF_STREAMS = 1;
        
        private IVMRWindowlessControl9 _pVMRWindowlessControl9;
        private IVMRFilterConfig9 _pVMRFilterConfig9;
        
        public VMR9Windowless()
        {
            _renderer = Renderer.VMR9_Windowless;
        }
        
        public override void SetVideoPosition(GDI.RECT rcSrc, GDI.RECT rcDest)
        {
            _pVMRWindowlessControl9.SetVideoPosition(/*ref rcSrc*/ IntPtr.Zero, ref rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int arWidth, out int arHeight)
        {
            _pVMRWindowlessControl9.GetNativeVideoSize(out width, out height, out arWidth, out arHeight);
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the VMR9 to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "VMR9 (Windowless)");
            errorFunc(hr, GraphBuilderError.AddVMR9);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the VMR9 interfaces
            try
            {
                _pVMRFilterConfig9 = (IVMRFilterConfig9)BaseFilter;
                _pVMRFilterConfig9.SetRenderingMode(VMR9Mode.VMR9Mode_Windowless);
                _pVMRFilterConfig9.SetNumberOfStreams(NUMBER_OF_STREAMS);
                _pVMRWindowlessControl9 = (IVMRWindowlessControl9)BaseFilter;
                _pVMRWindowlessControl9.SetVideoClippingWindow(hMediaWindow);

                _pVMRWindowlessControl9.SetAspectRatioMode(VMR9AspectRatioMode.VMR9ARMode_None);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.ConfigureVMR9, e);
            }
        }

        protected override void CloseInterfaces()
        {
            _pVMRFilterConfig9 = null;
            _pVMRWindowlessControl9 = null; // they will be released when pBaseFilter is released
            
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
            TraceSink.GetTraceSink().TraceWarning("Failed to instantiate VMR9.");
        }

        public IVMRWindowlessControl9 VMRWindowlessControl
        {
            get { return _pVMRWindowlessControl9; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IVMRFilterConfig9).GUID; }
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int hr = _pVMRWindowlessControl9.GetCurrentImage(out dibFull);
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

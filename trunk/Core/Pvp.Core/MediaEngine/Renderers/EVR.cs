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
    internal class EVR : RendererBase, IEVR
    {
        private IMFVideoDisplayControl _pMfVideoDisplayControl;
        private MFVideoNormalizedRect _rcSrc;
        private GDI.RECT _rcDest;
        
        public EVR()
        {
            _renderer = Renderer.EVR;

            _rcSrc = new MFVideoNormalizedRect();
            _rcSrc.left = _rcSrc.top = 0.0f;
            _rcSrc.right = _rcSrc.bottom = 1.0f;

            _rcDest = new GDI.RECT();
            _rcDest.left = _rcDest.top = 0;
        }
        
        public override void SetVideoPosition(GDI.RECT rcSrc, GDI.RECT rcDest)
        {
            // in EVR default source rectangle is {0.0, 0.0, 1.0, 1.0}, these are so-called normalized coordinates
            // however VMR, VMR9 and PVP consider the source rectangle as the video size
            // so we will just pass the default one to display the whole video frame
            
            // When we set video frame to be less than our windows EVR starts flickering in the surrounding areas, looks like some old content from
            // back buffers is being drawn
            // To overcme this issue we set our media window (nwnd) to be of the size of the video we want to show, in other words EVR should paint the whole window area
            // EVR's default destination rectangle is {0, 0, 0, 0} so we need to adjust it to {0, 0, width, height}
            _rcDest.right = rcDest.right - rcDest.left;
            _rcDest.bottom = rcDest.bottom - rcDest.top;
            _pMfVideoDisplayControl.SetVideoPosition(ref _rcSrc, ref _rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int arWidth, out int arHeight)
        {
            GDI.SIZE size = new GDI.SIZE(), ratio = new GDI.SIZE();
            _pMfVideoDisplayControl.GetNativeVideoSize(ref size, ref ratio);
            width = size.cx;
            height = size.cy;
            arWidth = ratio.cx;
            arHeight = ratio.cy;
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the EVR to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "Enhanced Video Renderer");
            errorFunc(hr, GraphBuilderError.AddEVR);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the EVR interfaces
            try
            {
                IMFGetService pMFGetService = (IMFGetService)BaseFilter; // will be released when IBaseFilter is released
                object o;
                Guid serviceId = ServiceID.EnhancedVideoRenderer;
                Guid IID_IMFVideoDisplayControl = typeof(IMFVideoDisplayControl).GUID;
                Marshal.ThrowExceptionForHR(pMFGetService.GetService(ref serviceId, ref IID_IMFVideoDisplayControl, out o));
                _pMfVideoDisplayControl = (IMFVideoDisplayControl)o;

                _pMfVideoDisplayControl.SetVideoWindow(hMediaWindow);
                _pMfVideoDisplayControl.SetAspectRatioMode(MFVideoAspectRatioMode.MFVideoARMode_None);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.ConfigureEVR, e);
            }
        }

        protected override Guid RendererID
        {
            get { return Clsid.EnhancedVideoRenderer; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            TraceSink.GetTraceSink().TraceWarning("Failed to instantiate EVR.");
        }

        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        protected override void CloseInterfaces()
        {
            if (_pMfVideoDisplayControl != null)
            {
                _pMfVideoDisplayControl.SetVideoWindow(IntPtr.Zero);
                Marshal.FinalReleaseComObject(_pMfVideoDisplayControl);
                _pMfVideoDisplayControl = null;
            }

            base.CloseInterfaces(); // release pBaseFilter
        }

        public IMFVideoDisplayControl MFVideoDisplayControl
        {
            get { return _pMfVideoDisplayControl; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IEVRFilterConfig).GUID; }
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int cbDib;
            long timestamp = 0;
            header = new BITMAPINFOHEADER();
            header.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            int hr = _pMfVideoDisplayControl.GetCurrentImage(ref header, out dibFull, out cbDib, ref timestamp);
            if (DsHlp.SUCCEEDED(hr))
            {
                dibDataOnly = new IntPtr(dibFull.ToInt64() + Marshal.SizeOf(typeof(BITMAPINFOHEADER)));
                return true;
            }
            else
            {
                dibDataOnly = IntPtr.Zero;
                return false;
            }
        }
    }
}

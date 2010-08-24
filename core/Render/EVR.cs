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
using System.Runtime.InteropServices;
using Dzimchuk.DirectShow;
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal class EVR : RendererBase
    {
        private IMFVideoDisplayControl pMFVideoDisplayControl;
        private MFVideoNormalizedRect rcSrc;
        private GDI.RECT rcDest;
        
        public EVR()
        {
            _renderer = Renderer.EVR;

            rcSrc = new MFVideoNormalizedRect();
            rcSrc.left = rcSrc.top = 0.0f;
            rcSrc.right = rcSrc.bottom = 1.0f;

            rcDest = new GDI.RECT();
            rcDest.left = rcDest.top = 0;
        }
        
        public override void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest)
        {
            // in EVR default source rectangle is {0.0, 0.0, 1.0, 1.0}, these are so-called normalized coordinates
            // however VMR, VMR9 and PVP consider the source rectangle as the video size
            // so we will just pass the default one to display the whole video frame
            
            // When we set video frame to be less than our windows EVR starts flickering in the surrounding areas, looks like some old content from
            // back buffers is being drawn
            // To overcme this issue we set our media window (nwnd) to be of the size of the video we want to show, in other words EVR should paint the whole window area
            // EVR's default destination rectangle is {0, 0, 0, 0} so we need to adjust it to {0, 0, width, height}
            this.rcDest.right = rcDest.right - rcDest.left;
            this.rcDest.bottom = rcDest.bottom - rcDest.top;
            pMFVideoDisplayControl.SetVideoPosition(ref this.rcSrc, ref this.rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight)
        {
            GDI.SIZE size = new GDI.SIZE(), ratio = new GDI.SIZE();
            pMFVideoDisplayControl.GetNativeVideoSize(ref size, ref ratio);
            width = size.cx;
            height = size.cy;
            ARWidth = ratio.cx;
            ARHeight = ratio.cy;
        }

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the EVR to the graph
            int hr = pGraphBuilder.AddFilter(pBaseFilter, "Enhanced Video Renderer");
            errorFunc(hr, Error.AddEVR);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            // QUERY the EVR interfaces
            try
            {
                IMFGetService pMFGetService = (IMFGetService)pBaseFilter; // will be released when IBaseFilter is released
                object o;
                Guid serviceId = ServiceID.EnhancedVideoRenderer;
                Guid IID_IMFVideoDisplayControl = typeof(IMFVideoDisplayControl).GUID;
                Marshal.ThrowExceptionForHR(pMFGetService.GetService(ref serviceId, ref IID_IMFVideoDisplayControl, out o));
                pMFVideoDisplayControl = (IMFVideoDisplayControl)o;

                pMFVideoDisplayControl.SetVideoWindow(hMediaWindow);
                pMFVideoDisplayControl.SetAspectRatioMode(MFVideoAspectRatioMode.MFVideoARMode_None);
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.ConfigureEVR, e);
            }
        }

        protected override Guid RendererID
        {
            get { return Clsid.EnhancedVideoRenderer; }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            Trace.GetTrace().TraceWarning("Failed to instantiate EVR.");
        }

        protected override bool IsDelayedInitialize
        {
            get { return false; }
        }

        protected override void CloseInterfaces()
        {
            if (pMFVideoDisplayControl != null)
            {
                pMFVideoDisplayControl.SetVideoWindow(IntPtr.Zero);
                Marshal.FinalReleaseComObject(pMFVideoDisplayControl);
                pMFVideoDisplayControl = null;
            }

            base.CloseInterfaces(); // release pBaseFilter
        }

        public IMFVideoDisplayControl MFVideoDisplayControl
        {
            get { return pMFVideoDisplayControl; }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get { return typeof(IEVRFilterConfig).GUID; }
        }
    }
}

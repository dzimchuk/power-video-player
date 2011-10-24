﻿/* ****************************************************************************
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
using System.Runtime.InteropServices;

namespace Dzimchuk.MediaEngine.Core.Render
{
    internal abstract class WindowedRenderer : RendererBase
    {
        protected IVideoWindow pVideoWindow;
        protected IBasicVideo2 pBasicVideo2;
        private bool _initialized = false;

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            try
            {
                pBasicVideo2 = (IBasicVideo2)pGraphBuilder;
                pVideoWindow = (IVideoWindow)pGraphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.NecessaryInterfaces, e);
            }

            pVideoWindow.put_Owner(hMediaWindow);
            pVideoWindow.put_MessageDrain(hMediaWindow);
            pVideoWindow.put_WindowStyle(WindowsManagement.WS_CHILD |
                WindowsManagement.WS_CLIPSIBLINGS);
        }

        protected override void CloseInterfaces()
        {
            if (pVideoWindow != null)
            {
                pVideoWindow.put_Visible(DsHlp.OAFALSE);
                pVideoWindow.put_MessageDrain(IntPtr.Zero);
                pVideoWindow.put_Owner(IntPtr.Zero);
                pVideoWindow = null;
            }

            pBasicVideo2 = null; // both interfaces are going to be released when pGraphBuilder is released

            base.CloseInterfaces(); // must be called to release the main pointer and all child interfaces (if any)
        }

        protected override bool IsDelayedInitialize
        {
            get { return true; }
        }

        public override void SetVideoPosition(ref GDI.RECT rcSrc, ref GDI.RECT rcDest)
        {
            if (!_initialized)
            {
                Initialize(pGraphBuilder, hMediaWindow);
                _initialized = true;
            }
            
            pVideoWindow.SetWindowPosition(rcDest.left, rcDest.top,
                        rcDest.right - rcDest.left,
                        rcDest.bottom - rcDest.top);
            pBasicVideo2.SetDefaultDestinationPosition();
        }

        public override void GetNativeVideoSize(out int width, out int height, out int ARWidth, out int ARHeight)
        {
            if (!_initialized)
            {
                Initialize(pGraphBuilder, hMediaWindow);
                _initialized = true;
            }
            
            pBasicVideo2.GetVideoSize(out width, out height);
            pBasicVideo2.GetPreferredAspectRatio(out ARWidth, out ARHeight);
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int bufferSize = 0;
            int hr = pBasicVideo2.GetCurrentImage(ref bufferSize, IntPtr.Zero); // get the required buffer size first
            if (DsHlp.SUCCEEDED(hr))
            {
                dibFull = Marshal.AllocCoTaskMem(bufferSize);
                hr = pBasicVideo2.GetCurrentImage(ref bufferSize, dibFull); // actually get the image
                if (DsHlp.SUCCEEDED(hr))
                {
                    header = (BITMAPINFOHEADER)Marshal.PtrToStructure(dibFull, typeof(BITMAPINFOHEADER));
                    dibDataOnly = new IntPtr(dibFull.ToInt64() + Marshal.SizeOf(typeof(BITMAPINFOHEADER)));
                    return true;
                }
            }

            header = new BITMAPINFOHEADER();
            dibDataOnly = IntPtr.Zero;
            dibFull = IntPtr.Zero;
            return false;
        }
    }
}

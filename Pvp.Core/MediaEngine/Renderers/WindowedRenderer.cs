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
using Pvp.Core.Native;
using System.Runtime.InteropServices;

namespace Pvp.Core.MediaEngine.Renderers
{
    internal abstract class WindowedRenderer : RendererBase
    {
        protected IVideoWindow VideoWindow;
        protected IBasicVideo2 BasicVideo2;
        private bool _initialized;

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            try
            {
                BasicVideo2 = (IBasicVideo2)pGraphBuilder;
                VideoWindow = (IVideoWindow)pGraphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NecessaryInterfaces, e);
            }

            VideoWindow.put_Owner(hMediaWindow);
            VideoWindow.put_MessageDrain(hMediaWindow);
            VideoWindow.put_WindowStyle(WindowsManagement.WS_CHILD |
                WindowsManagement.WS_CLIPSIBLINGS);
        }

        protected override void CloseInterfaces()
        {
            if (VideoWindow != null)
            {
                VideoWindow.put_Visible(DsHlp.OAFALSE);
                VideoWindow.put_MessageDrain(IntPtr.Zero);
                VideoWindow.put_Owner(IntPtr.Zero);
                VideoWindow = null;
            }

            BasicVideo2 = null; // both interfaces are going to be released when _pGraphBuilder is released

            base.CloseInterfaces(); // must be called to release the main pointer and all child interfaces (if any)
        }

        protected override bool IsDelayedInitialize
        {
            get { return true; }
        }

        public override void SetVideoPosition(GDI.RECT rcSrc, GDI.RECT rcDest)
        {
            if (!_initialized)
            {
                Initialize(GraphBuilder, MediaWindowHandle);
                _initialized = true;
            }
            
            VideoWindow.SetWindowPosition(rcDest.left, rcDest.top,
                        rcDest.right - rcDest.left,
                        rcDest.bottom - rcDest.top);
            BasicVideo2.SetDefaultDestinationPosition();
        }

        public override void GetNativeVideoSize(out int width, out int height, out int arWidth, out int arHeight)
        {
            if (!_initialized)
            {
                Initialize(GraphBuilder, MediaWindowHandle);
                _initialized = true;
            }
            
            BasicVideo2.GetVideoSize(out width, out height);
            BasicVideo2.GetPreferredAspectRatio(out arWidth, out arHeight);
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int bufferSize = 0;
            int hr = BasicVideo2.GetCurrentImage(ref bufferSize, IntPtr.Zero); // get the required buffer size first
            if (DsHlp.SUCCEEDED(hr))
            {
                dibFull = Marshal.AllocCoTaskMem(bufferSize);
                hr = BasicVideo2.GetCurrentImage(ref bufferSize, dibFull); // actually get the image
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

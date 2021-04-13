using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Pvp.Core.Wpf
{
    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("8F911837-FF4A-4C38-87F8-02EC6B05785A")]
    internal interface IPvpPresenter
    {
        [PreserveSig]
        int HasNewSurfaceArrived(out bool newSurfaceArrived);

        [PreserveSig]
        int GetBackBufferNoRef(out IntPtr pSurface);
    }

    internal class PvpPresenter : IPvpPresenterHook
    {
        private readonly D3DImage _d3dImage;
        private IPvpPresenter _pvpPresenter;
        private TimeSpan _lastRender;
        private bool _hookedUp;
        
        public PvpPresenter(D3DImage d3dImage)
        {
            _d3dImage = d3dImage;
        }

        public void HookUp(object rcwPresenter)
        {
            _pvpPresenter = (IPvpPresenter)rcwPresenter;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            _hookedUp = true;
        }

        public void Dispose()
        {
            _pvpPresenter = null; // it's released when the renderer is finalized
            if (_hookedUp)
                CompositionTarget.Rendering -= CompositionTarget_Rendering;

            _hookedUp = false;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.

            if (_d3dImage.IsFrontBufferAvailable && _lastRender != args.RenderingTime)
            {
                bool newSurfaceArrived;
                _pvpPresenter.HasNewSurfaceArrived(out newSurfaceArrived);
                if (newSurfaceArrived)
                {
                    _d3dImage.Lock();

                    IntPtr pSurface;
                    _pvpPresenter.GetBackBufferNoRef(out pSurface);

                    if (pSurface != null)
                    {
                        // Repeatedly calling SetBackBuffer with the same IntPtr is 
                        // a no-op. There is no performance penalty.
                        _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);

                        _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                    }

                    _d3dImage.Unlock();
                }

                _lastRender = args.RenderingTime;
                //System.Diagnostics.Debug.WriteLine("{0} : Repaint: {1}", args.RenderingTime, newSurfaceArrived);
            }
        }
    }
}
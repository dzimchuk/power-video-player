using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Interop;
using System.Windows;

namespace Pvp.Core.Wpf
{
    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("97229B96-8BB6-4666-849D-680DA312977A")]
    internal interface IPvpPresenter2
    {
        [PreserveSig]
        int RegisterCallback(IPvpPresenterCallback pCallback);

        [PreserveSig]
        int GetBackBufferNoRef(out IntPtr pSurface);
    }

    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("90D15027-388A-44AB-AADF-733D3BCDBC7B")]
    internal interface IPvpPresenterCallback
    {
        [PreserveSig]
        int OnNewSurfaceArrived();
    }

    internal class PvpPresenter2 : IPvpPresenterHook, IPvpPresenterCallback
    {
        private readonly D3DImage _d3dImage;
        private IPvpPresenter2 _pvpPresenter;

        public PvpPresenter2(D3DImage d3dImage)
        {
            _d3dImage = d3dImage;
        }

        public void HookUp(object rcwPresenter)
        {
            _pvpPresenter = (IPvpPresenter2)rcwPresenter;
            _pvpPresenter.RegisterCallback(this);
        }

        public void Dispose()
        {
            _pvpPresenter = null; // it's released when the renderer is finalized
        }

        public int OnNewSurfaceArrived()
        {
            _d3dImage.Dispatcher.Invoke(new Action(() =>
            {
                if (_d3dImage.IsFrontBufferAvailable)
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
            }), System.Windows.Threading.DispatcherPriority.Send);
            
            return 0;
        }
    }
}

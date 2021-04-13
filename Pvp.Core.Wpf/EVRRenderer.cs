using System;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;
using Pvp.Core.MediaEngine.Renderers;
using Pvp.Core.Native;

namespace Pvp.Core.Wpf
{
    [ComVisible(true), ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("B3C97321-2C16-457D-AEBC-8AFA7BC9CE4A")]
    internal interface IPvpPresenterConfig
    {
        [PreserveSig]
        int SetBufferCount(int bufferCount);
    }

    internal class EVRRenderer : RendererBase, IEVR
    {
        private const int PRESENTER_BUFFER_COUNT = 4;
        private static Guid CLSID_CustomEVRPresenter = new Guid("4C536A77-7D8B-4F92-ACCA-27F84912B393");

        private IMFVideoDisplayControl _pMFVideoDisplayControl;
        private IPvpPresenterConfig _pvpPresenterConfig;
        
        private MFVideoNormalizedRect _rcSrc;
        private GDI.RECT _rcDest;

        private readonly IPvpPresenterHook _pvpPresenterHook;
        
        public EVRRenderer(IPvpPresenterHook pvpPresenterHook)
        {
            _pvpPresenterHook = pvpPresenterHook;
            _renderer = MediaEngine.Renderer.EVR;

            _rcSrc = new MFVideoNormalizedRect { left = _rcSrc.top = 0.0f, right = _rcSrc.bottom = 1.0f };

            _rcDest = new GDI.RECT { left = _rcDest.top = 0 };
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
            _pMFVideoDisplayControl.SetVideoPosition(ref _rcSrc, ref _rcDest);
        }

        public override void GetNativeVideoSize(out int width, out int height, out int arWidth, out int arHeight)
        {
            GDI.SIZE size = new GDI.SIZE(), ratio = new GDI.SIZE();
            _pMFVideoDisplayControl.GetNativeVideoSize(ref size, ref ratio);
            width = size.cx;
            height = size.cy;
            arWidth = ratio.cx;
            arHeight = ratio.cy;
        }

        public override bool GetCurrentImage(out BITMAPINFOHEADER header, out IntPtr dibFull, out IntPtr dibDataOnly)
        {
            int cbDib;
            long timestamp = 0;
            header = new BITMAPINFOHEADER();
            header.biSize = Marshal.SizeOf(typeof (BITMAPINFOHEADER));
            int hr = _pMFVideoDisplayControl.GetCurrentImage(ref header, out dibFull, out cbDib, ref timestamp);
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

        protected override void AddToGraph(IGraphBuilder pGraphBuilder, ThrowExceptionForHRPointer errorFunc)
        {
            // add the EVR to the graph
            int hr = pGraphBuilder.AddFilter(BaseFilter, "Enhanced Video Renderer");
            errorFunc(hr, GraphBuilderError.AddEVR);
        }

        protected override void Initialize(IGraphBuilder pGraphBuilder, IntPtr hMediaWindow)
        {
            object factoryObject = null;
            object presenterObject = null;
            try
            {
                int hr = ClassFactory.GetEvrPresenterClassFactory(ref CLSID_CustomEVRPresenter, ref ClassFactory.IID_ClassFactory, out factoryObject);
                Marshal.ThrowExceptionForHR(hr);

                IClassFactory factory = (IClassFactory)factoryObject;

                var iidPresenter = typeof(IMFVideoPresenter).GUID;
                hr = factory.CreateInstance(null, ref iidPresenter, out presenterObject);
                Marshal.ThrowExceptionForHR(hr);

                IMFVideoPresenter presenter = (IMFVideoPresenter)presenterObject;

                IMFVideoRenderer renderer = (IMFVideoRenderer)BaseFilter; // will be released when IBaseFilter is released
                renderer.InitializeRenderer(null, presenter);

                IMFGetService pMFGetService = (IMFGetService)BaseFilter; // will be released when IBaseFilter is released
                object o;
                var serviceId = ServiceID.EnhancedVideoRenderer;
                var iidImfVideoDisplayControl = typeof(IMFVideoDisplayControl).GUID;
                Marshal.ThrowExceptionForHR(pMFGetService.GetService(ref serviceId, ref iidImfVideoDisplayControl, out o));
                _pMFVideoDisplayControl = (IMFVideoDisplayControl)o;

                _pMFVideoDisplayControl.SetVideoWindow(hMediaWindow);
                _pMFVideoDisplayControl.SetAspectRatioMode(MFVideoAspectRatioMode.MFVideoARMode_None);

                _pvpPresenterConfig = (IPvpPresenterConfig)presenterObject;
                _pvpPresenterConfig.SetBufferCount(PRESENTER_BUFFER_COUNT);

                _pvpPresenterHook.HookUp(presenterObject);

                // as EVR requests IMFVideoDisplayControl from the presenter and our custom presenter implements IPvpPresenter and IPvpPresenterConfig
                // presenterObject and _pMFVideoDisplayControl point to the same RCW

                presenterObject = null; // we will release the presenter when releasing _pMFVideoDisplayControl
            }
            catch
            {
                _pMFVideoDisplayControl = null;
                _pvpPresenterConfig = null;
            }
            finally
            {
                if (factoryObject != null)
                    Marshal.FinalReleaseComObject(factoryObject);

                if (presenterObject != null)
                    Marshal.FinalReleaseComObject(presenterObject);
            }
        }

        protected override Guid RendererID
        {
            get
            {
                return Clsid.EnhancedVideoRenderer;
            }
        }

        protected override void HandleInstantiationError(Exception e)
        {
            // TODO: log it
        }

        protected override bool IsDelayedInitialize
        {
            get
            {
                return false;
            }
        }

        protected override Guid IID_4DVDGraphInstantiation
        {
            get
            {
                return typeof(IEVRFilterConfig).GUID;
            }
        }

        public IMFVideoDisplayControl MFVideoDisplayControl
        {
            get
            {
                return _pMFVideoDisplayControl;
            }
        }

        protected override void CloseInterfaces()
        {
            if (_pMFVideoDisplayControl != null)
            {
                _pMFVideoDisplayControl.SetVideoWindow(IntPtr.Zero);
                Marshal.FinalReleaseComObject(_pMFVideoDisplayControl);

                _pMFVideoDisplayControl = null;
                _pvpPresenterConfig = null;
            }

            base.CloseInterfaces(); // release pBaseFilter
        }
    }
}
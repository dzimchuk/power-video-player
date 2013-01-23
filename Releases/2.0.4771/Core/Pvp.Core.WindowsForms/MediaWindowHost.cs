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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Pvp.Core.Native;
using Pvp.Core.MediaEngine;
using Pvp.Core.Nwnd;

namespace Pvp.Core.WindowsForms
{
    /// <summary>
    /// A control that hosts a media window.
    /// </summary>
    public abstract class MediaWindowHost : System.Windows.Forms.UserControl, IMediaWindowHost
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container _components = null;

        public event EventHandler MWDoubleClick;
        public event EventHandler MWClick;
        public event ContextMenuHandler MWContextMenu;
        public event EventHandler MWMouseEnter;
        public event EventHandler MWMouseLeave;
        public event EventHandler MWMouseMove;

        private MediaWindow _mediaWindow;
        private IMediaEngine _engine;
        private bool _showLogo;

        private readonly MediaWindowHandler _mwHandler;
        private bool _mouseOnWindow;

        private GDI.RECT _rcClient;
        private GDI.SIZE _rcSrc;    // native video size
        private GDI.RECT _rcDest;   // video destination rectangle relative to the media window host
        private GDI.RECT _rcDestMW; // video destination rectangle relative to the media window (i.e. top and left are always 0)
        private double _nativeAspectRatio;

        private AspectRatio _aspectRatio = AspectRatio.AR_ORIGINAL;

        // Video Size stuff
        private const int DIVIDESIZE50 = 2;
        private bool _isFixed = true;		                //FIXED (true) of FREE (false)
        private VideoSize _fixedSize = VideoSize.SIZE100;	//FIXED video size (SIZE100 or SIZE 200)
        private int _divideSize = 1;

        #region MediaWindowHook
        private class MediaWindowHandler
        {
            private bool _doubleClick; // fix for extra mouse up message we want to discard
            private bool _trackingContextMenu; // fix for additional WM_CONTEXTMENU from MediaWindow when it's already sent by nwnd
            private uint _previousMousePosition; // fix against spurious WM_MOUSEMOVE messages, see http://blogs.msdn.com/oldnewthing/archive/2003/10/01/55108.aspx#55109
            readonly MediaWindowHost _mwh;
            readonly IMediaWindowHost _host;

            public MediaWindowHandler(MediaWindowHost mwh)
            {
                _mwh = mwh;
                _host = mwh;
            }

            public void HandleMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
            {
                switch (Msg)
                {
                    case (uint)WindowsMessages.WM_LBUTTONDBLCLK:
                        _doubleClick = true;
                        if (_mwh.MWDoubleClick != null)
                            _mwh.MWDoubleClick(_mwh, EventArgs.Empty);
                        break;
                    case (uint)WindowsMessages.WM_CONTEXTMENU:
                        if (!_trackingContextMenu)
                        {
                            _trackingContextMenu = true;
                            if (_mwh.MWContextMenu != null)
                                _mwh.MWContextMenu(Cursor.Position);
                        }
                        else
                            _trackingContextMenu = false;
                        break;
                    case (uint)WindowsMessages.WM_LBUTTONUP:
                        {
                            IMediaWindow mediaWindow = _host.GetMediaWindow();
                            if (mediaWindow != null && mediaWindow.Handle == hWnd && _mwh._engine.IsMenuOn)
                            {
                                uint mylParam = (uint)lParam;
                                uint x = mylParam & 0x0000FFFF;
                                uint y = mylParam & 0xFFFF0000;
                                y >>= 16;

                                GDI.POINT pt = new GDI.POINT();
                                pt.x = (int)x;
                                pt.y = (int)y;
                                _mwh._engine.ActivateDVDMenuButtonAtPosition(pt);
                            }

                            if (!_mwh._engine.IsMenuOn && !_doubleClick && _mwh.MWClick != null)
                                _mwh.MWClick(_mwh, EventArgs.Empty);
                            _doubleClick = false;
                        }
                        break;
                    case (uint)WindowsMessages.WM_MOUSEMOVE:
                        if ((uint)lParam != _previousMousePosition) // mouse was actually moved as its position has changed
                        {
                            _previousMousePosition = (uint)lParam;

                            IMediaWindow mediaWindow = _host.GetMediaWindow();
                            if (mediaWindow != null && mediaWindow.Handle == hWnd && _mwh._engine.IsMenuOn)
                            {
                                uint mylParam = (uint)lParam;
                                uint x = mylParam & 0x0000FFFF;
                                uint y = mylParam & 0xFFFF0000;
                                y >>= 16;

                                GDI.POINT pt = new GDI.POINT();
                                pt.x = (int)x;
                                pt.y = (int)y;
                                _mwh._engine.SelectDVDMenuButtonAtPosition(pt);
                            }

                            if (!_mwh._mouseOnWindow)
                            {
                                WindowsManagement.TRACKMOUSEEVENT tme =
                                    new WindowsManagement.TRACKMOUSEEVENT();
                                tme.cbSize = Marshal.SizeOf(tme);
                                tme.dwFlags = WindowsManagement.TME_LEAVE;
                                tme.dwHoverTime = WindowsManagement.HOVER_DEFAULT;
                                tme.hwndTrack = hWnd;

                                WindowsManagement._TrackMouseEvent(ref tme);
                                _mwh._mouseOnWindow = true;
                                if (_mwh.MWMouseEnter != null)
                                    _mwh.MWMouseEnter(_mwh, EventArgs.Empty);
                            }

                            if (_mwh.MWMouseMove != null)
                                _mwh.MWMouseMove(_mwh, EventArgs.Empty);
                        }
                        break;
                    case (uint)WindowsMessages.WM_MOUSELEAVE:
                        _mwh._mouseOnWindow = false;
                        if (_mwh.MWMouseLeave != null)
                            _mwh.MWMouseLeave(_mwh, EventArgs.Empty);
                        break;
                }
            }
        }

        private void _mediaWindow_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (_mwHandler != null)
                _mwHandler.HandleMessage(e.HWnd, e.Msg, e.WParam, e.LParam);
        }

        protected override void WndProc(ref Message m)
        {
            if (_mwHandler != null)
                _mwHandler.HandleMessage(m.HWnd, (uint)m.Msg, m.WParam, m.LParam);
            base.WndProc(ref m);
        }
        #endregion

        public MediaWindowHost()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            SetStyle(ControlStyles.Selectable, false);

            _mwHandler = new MediaWindowHandler(this);

            BackColor = Color.Black;

            InitializeMediaEngine();
        }

        private void InitializeMediaEngine()
        {
            _engine = MediaEngineServiceProvider.GetMediaEngine(this);
            _engine.MediaWindowDisposed += delegate(object sender, EventArgs args)
            {
                if (_mediaWindow != null)
                {
                    _mediaWindow.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
                }

                CreateMediaWindow();
            };

            _engine.InitSize += (sender, args) =>
            {
                _rcSrc = args.NewVideSize;
                _nativeAspectRatio = args.NativeAspectRatio;

                if (args.Initial)
                    OnInitSize(new InitSizeEventArgs(new Size(args.NewVideSize.cx, args.NewVideSize.cy)));

                ResizeNormal(args.Initial);

                if (args.InvalidateSuggested)
                    InvalidateMediaWindow();
            };
        }

        protected virtual void OnInitSize(InitSizeEventArgs args)
        {
            if (InitSize != null)
                InitSize(this, args);
        }

        protected IMediaEngine MediaEngine
        {
            get { return _engine; }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            CreateMediaWindow();
            base.OnHandleCreated(e);
        }

        private void CreateMediaWindow()
        {
            _mediaWindow = new MediaWindow(Handle, 0, 0, Width, Height, 
                WindowsManagement.WS_VISIBLE | WindowsManagement.WS_CHILD | WindowsManagement.WS_CLIPSIBLINGS);
            _mediaWindow.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);

            if (_bitmap != null)
                _mediaWindow.SetLogo(_bitmap.GetHbitmap()); // creates new GDI bitmap object that will be destroyed in media window's destructor
            _mediaWindow.ShowLogo(_showLogo);
        }

        private void InvalidateMediaWindow()
        {
            if (_mediaWindow != null)
                _mediaWindow.Invalidate();
        }

        public new Control Parent
        {
            get { return base.Parent; }
            set
            {
                value.KeyDown += new KeyEventHandler(OnParentKeyDown);
                base.Parent = value;
            }
        }

        private void OnParentKeyDown(object sender, KeyEventArgs e)
        {
            if (_engine.IsMenuOn)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        _engine.ActivateSelectedDVDMenuButton();
                        break;
                    case Keys.Left:
                        _engine.SelectDVDMenuButtonLeft();
                        break;
                    case Keys.Right:
                        _engine.SelectDVDMenuButtonRight();
                        break;
                    case Keys.Up:
                        _engine.SelectDVDMenuButtonUp();
                        break;
                    case Keys.Down:
                        _engine.SelectDVDMenuButtonDown();
                        break;
                }
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_components != null)
                {
                    _components.Dispose();
                }

                if (_bitmap != null)
                    _bitmap.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_mediaWindow != null)
            {
                if (_engine.GraphState == GraphState.Reset)
                {
                    _rcClient.right = Width;
                    _rcClient.bottom = Height;
                    _mediaWindow.Move(ref _rcClient); // resize to the full client area to center the logo
                }
                else
                {
                    ResizeNormal();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //	base.OnPaint (e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        #region Resizing stuff
        private void ResizeNormal(bool initial = false)
        {
            if (!initial && _engine.GraphState == GraphState.Reset)
                return;

            GDI.RECT rect;
            WindowsManagement.GetClientRect(Handle, out rect);
            int clientWidth = rect.right - rect.left;
            int clientHeight = rect.bottom - rect.top;

            double w = clientWidth;
            double h = clientHeight;
            double ratio = w / h;
            double dAspectRatio;

            switch (_aspectRatio)
            {
                case AspectRatio.AR_ORIGINAL:
                    dAspectRatio = _nativeAspectRatio;
                    break;
                case AspectRatio.AR_16x9:
                    dAspectRatio = 16.0 / 9.0;
                    break;
                case AspectRatio.AR_4x3:
                    dAspectRatio = 4.0 / 3.0;
                    break;
                case AspectRatio.AR_47x20:
                    dAspectRatio = 47.0 / 20.0;
                    break;
                default:
                    {
                        // free aspect ratio
                        _rcDest.left = 0;
                        _rcDest.top = 0;
                        _rcDest.right = clientWidth;
                        _rcDest.bottom = clientHeight;
                        ApplyDestinationRect();
                        return;
                    }
            }

            int hor;
            int vert;

            if (_isFixed)
            {
                int fixedSize = (int)_fixedSize;
                if (ratio >= dAspectRatio)
                {
                    vert = ((int)(_rcSrc.cy * fixedSize / _divideSize)) - clientHeight;
                    _rcDest.top = (vert >= 0) ? 0 : -vert / 2;
                    _rcDest.bottom = (vert >= 0) ? clientHeight : _rcDest.top + ((int)(_rcSrc.cy * fixedSize / _divideSize));
                    h = _rcDest.bottom - _rcDest.top;
                    w = h * dAspectRatio;
                    hor = clientWidth - (int)w;
                    _rcDest.left = (hor <= 0) ? 0 : hor / 2;
                    _rcDest.right = _rcDest.left + (int)w;
                }
                else
                {
                    hor = ((int)(_rcSrc.cx * fixedSize / _divideSize)) - clientWidth;
                    // hor>=0 - client area is smaller than video hor size
                    _rcDest.left = (hor >= 0) ? 0 : -hor / 2;
                    _rcDest.right = (hor >= 0) ? clientWidth : _rcDest.left + ((int)(_rcSrc.cx * fixedSize / _divideSize));
                    w = _rcDest.right - _rcDest.left;
                    h = w / dAspectRatio;
                    vert = clientHeight - (int)h;
                    _rcDest.top = (vert <= 0) ? 0 : vert / 2;
                    _rcDest.bottom = _rcDest.top + (int)h;
                }

            }
            else
            {
                if (ratio >= dAspectRatio)
                {
                    _rcDest.top = 0;
                    _rcDest.bottom = clientHeight;
                    h = _rcDest.bottom - _rcDest.top;
                    w = h * dAspectRatio;
                    hor = clientWidth - (int)w;
                    _rcDest.left = (hor <= 0) ? 0 : hor / 2;
                    _rcDest.right = _rcDest.left + (int)w;
                }
                else
                {
                    _rcDest.left = 0;
                    _rcDest.right = clientWidth;
                    w = _rcDest.right - _rcDest.left;
                    h = w / dAspectRatio;
                    vert = clientHeight - (int)h;
                    _rcDest.top = (vert <= 0) ? 0 : vert / 2;
                    _rcDest.bottom = _rcDest.top + (int)h;
                }

            }

            ApplyDestinationRect();
        }

        private void ApplyDestinationRect()
        {
            // move the media window to the new position
            _mediaWindow.Move(ref _rcDest);

            // set the new rectangle on the renderer but relative to the media window;
            // as we now always resize the media window to fit the destinaton rectangle
            // we need to make sure Top and Left values are 0
            _rcDestMW.right = _rcDest.right - _rcDest.left;
            _rcDestMW.bottom = _rcDest.bottom - _rcDest.top;
            _engine.SetVideoPosition(ref _rcDestMW);
        }
        #endregion

        #region Public events

        /// <summary>
        /// Occurs when new video has been rendered and indicates that
        /// the application should resize to accomodate the new video frame.
        /// </summary>
        public event EventHandler<InitSizeEventArgs> InitSize;

        #endregion

        #region Public methods and properties

        public AspectRatio AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                _aspectRatio = value;
                ResizeNormal();
                InvalidateMediaWindow();
            }
        }

        public VideoSize VideoSize
        {
            get
            {
                VideoSize ret = VideoSize.SIZE_FREE;
                if (_isFixed)
                {
                    switch (_fixedSize)
                    {
                        case VideoSize.SIZE100:
                            {
                                ret = _divideSize == DIVIDESIZE50 ? VideoSize.SIZE50 : VideoSize.SIZE100;
                                break;
                            }
                        case VideoSize.SIZE200:
                            {
                                ret = VideoSize.SIZE200;
                                break;
                            }
                    }
                }
                return ret;
            }
            set
            {
                switch (value)
                {
                    case VideoSize.SIZE100:
                        {
                            _isFixed = true;
                            _fixedSize = VideoSize.SIZE100;
                            _divideSize = 1;
                            break;
                        }
                    case VideoSize.SIZE200:
                        {
                            _isFixed = true;
                            _fixedSize = VideoSize.SIZE200;
                            _divideSize = 1;
                            break;
                        }
                    case VideoSize.SIZE50:
                        {
                            _isFixed = true;
                            _fixedSize = VideoSize.SIZE100;
                            _divideSize = DIVIDESIZE50;
                            break;
                        }
                    default:
                        {
                            _isFixed = !_isFixed;
                            break;
                        }
                }

                ResizeNormal();
            }
        }

        public bool ShowLogo
        {
            get { return _showLogo; }
            set
            {
                _showLogo = value;              
            }
        }

        Bitmap _bitmap;
        public Bitmap Logo
        {
            set
            {
                _bitmap = value;
            }
        }

        #endregion

        IMediaWindow IMediaWindowHost.GetMediaWindow()
        {
            return _mediaWindow;
        }
    }
}

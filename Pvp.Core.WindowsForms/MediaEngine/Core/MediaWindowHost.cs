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
using Dzimchuk.Native;

namespace Dzimchuk.MediaEngine.Core
{
    public delegate void ContextMenuHandler(Point ptScreen);

    /// <summary>
    /// Media Window.
    /// </summary>
    public class MediaWindowHost : System.Windows.Forms.UserControl, IMediaWindowHost
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public event EventHandler MW_DoubleClick;
        public event EventHandler MW_Click;
        public event ContextMenuHandler MW_ContextMenu;
        public event EventHandler MW_MouseEnter;
        public event EventHandler MW_MouseLeave;
        public event EventHandler MW_MouseMove;

        private MediaWindow _mediaWindow;
        private IMediaEngine _engine;
        private bool _showLogo;

        private MediaWindowHandler _mwHandler;
        private bool _mouseOnWindow;

        private GDI.RECT _rcClient;

        #region MediaWindowHook
        private class MediaWindowHandler
        {
            private bool _doubleClick; // fix for extra mouse up message we want to discard
            private bool _trackingContextMenu; // fix for additional WM_CONTEXTMENU from MediaWindow when it's already sent by nwnd
            private uint _previousMousePosition; // fix against spurious WM_MOUSEMOVE messages, see http://blogs.msdn.com/oldnewthing/archive/2003/10/01/55108.aspx#55109
            MediaWindowHost _mwh;
            public MediaWindowHandler(MediaWindowHost mwh)
            {
                this._mwh = mwh;
            }

            public void HandleMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam)
            {
                switch (Msg)
                {
                    case (uint)WindowsMessages.WM_LBUTTONDBLCLK:
                        _doubleClick = true;
                        if (_mwh.MW_DoubleClick != null)
                            _mwh.MW_DoubleClick(_mwh, EventArgs.Empty);
                        break;
                    case (uint)WindowsMessages.WM_CONTEXTMENU:
                        if (!_trackingContextMenu)
                        {
                            _trackingContextMenu = true;
                            if (_mwh.MW_ContextMenu != null)
                                _mwh.MW_ContextMenu(Cursor.Position);
                        }
                        else
                            _trackingContextMenu = false;
                        break;
                    case (uint)WindowsMessages.WM_LBUTTONUP:
                        if (!_mwh._engine.IsMenuOn && !_doubleClick && _mwh.MW_Click != null)
                            _mwh.MW_Click(_mwh, EventArgs.Empty);
                        _doubleClick = false;
                        break;
                    case (uint)WindowsMessages.WM_MOUSEMOVE:
                        if ((uint)lParam != _previousMousePosition) // mouse was actually moved as its position has changed
                        {
                            _previousMousePosition = (uint)lParam;
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
                                if (_mwh.MW_MouseEnter != null)
                                    _mwh.MW_MouseEnter(_mwh, EventArgs.Empty);
                            }

                            if (_mwh.MW_MouseMove != null)
                                _mwh.MW_MouseMove(_mwh, EventArgs.Empty);
                        }
                        break;
                    case (uint)WindowsMessages.WM_MOUSELEAVE:
                        _mwh._mouseOnWindow = false;
                        if (_mwh.MW_MouseLeave != null)
                            _mwh.MW_MouseLeave(_mwh, EventArgs.Empty);
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

            _engine = MediaEngineServiceProvider.GetMediaEngine(this);
            _engine.MediaWindowDisposed += delegate(object sender, EventArgs args)
            {
                if (_mediaWindow != null)
                {
                    _mediaWindow.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
                }

                CreateMediaWindow();
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            CreateMediaWindow();
            base.OnHandleCreated(e);
        }

        private void CreateMediaWindow()
        {
            _mediaWindow = new MediaWindow(Handle, Width, Height);
            _mediaWindow.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
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
                if (components != null)
                {
                    components.Dispose();
                }
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
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_mediaWindow != null && _engine.GraphState == GraphState.Reset)
            {
                _rcClient.right = Width;
                _rcClient.bottom = Height;
                _mediaWindow.Move(ref _rcClient); // resize to the full client area to center the logo
            }
            _engine.OnMediaWindowHostResized();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //	base.OnPaint (e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        #region Public methods and properties

        public IMediaEngine MediaEngine
        {
            get { return _engine; }
        }

        public bool ShowLogo
        {
            get { return _showLogo; }
            set
            {
                _showLogo = value;
                MediaWindow.IsShowLogo(value);
                MediaWindow.InvalidateMediaWindow();
            }
        }

        public Bitmap Logo
        {
            set
            { 
                MediaWindow.SetLogo(value.GetHbitmap());
            }
        }

        #endregion

        IMediaWindow IMediaWindowHost.GetMediaWindow()
        {
            return _mediaWindow;
        }
    }
}

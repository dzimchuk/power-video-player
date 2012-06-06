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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Pvp.Core.MediaEngine;
using Pvp.Core.Native;
using Pvp.Core.Nwnd;

namespace Pvp.Core.Wpf
{
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public abstract class MediaWindowHost : Control, IMediaWindowHost
    {
        static MediaWindowHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaWindowHost), new FrameworkPropertyMetadata(typeof(MediaWindowHost)));
        }
  
        public static readonly DependencyProperty LogoBrushProperty =
            DependencyProperty.Register("LogoBrush", typeof(Brush), typeof(MediaWindowHost), new PropertyMetadata(default(Brush)));
  
        public static readonly DependencyProperty LogoMaxWidthProperty =
            DependencyProperty.Register("LogoMaxWidth", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));
  
        public static readonly DependencyProperty LogoMaxHeightProperty =
            DependencyProperty.Register("LogoMaxHeight", typeof(double), typeof(MediaWindowHost), new PropertyMetadata(double.PositiveInfinity));
  
        public static readonly RoutedEvent MWContextMenuEvent = EventManager.RegisterRoutedEvent("MWContextMenu", RoutingStrategy.Bubble,
            typeof(MWContextMenuEventHandler), typeof(MediaWindowHost));
  
        public static readonly RoutedEvent MWDoubleClickEvent = EventManager.RegisterRoutedEvent("MWDoubleClick", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MediaWindowHost));
  
        public static readonly RoutedEvent MWClickEvent = EventManager.RegisterRoutedEvent("MWClick", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MediaWindowHost));
  
        public static readonly RoutedEvent MWMouseEnterEvent = EventManager.RegisterRoutedEvent("MWMouseEnter", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MediaWindowHost));
  
        public static readonly RoutedEvent MWMouseLeaveEvent = EventManager.RegisterRoutedEvent("MWMouseLeave", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MediaWindowHost));
  
        public static readonly RoutedEvent MWMouseMoveEvent = EventManager.RegisterRoutedEvent("MWMouseMove", RoutingStrategy.Bubble,
            typeof(MWMouseMoveEventHandler), typeof(MediaWindowHost));

        public static readonly RoutedEvent InitSizeEvent = EventManager.RegisterRoutedEvent("InitSize", RoutingStrategy.Bubble,
            typeof(InitSizeEventHandler), typeof(MediaWindowHost));
  
        private Border _border;

        private MediaWindowHwndHost _hwndHost;
        private IMediaWindow _mediaWindow;

        private readonly MediaWindowHandler _mwHandler;

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
  
        protected MediaWindowHost()
        {
            _mwHandler = new MediaWindowHandler(this);
            InitializeMediaEngine();

            Application.Current.MainWindow.LocationChanged += new EventHandler(MainWindow_LocationChanged);
        }
  
        private void InitializeMediaEngine()
        {
            MediaEngine = MediaEngineServiceProvider.GetMediaEngine(this);
            MediaEngine.MediaWindowDisposed += delegate(object sender, EventArgs args)
            {
                if (_mediaWindow != null)
                {
                    _mediaWindow.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
                    _mediaWindow = null;
                }

                SetMediaWindowState(false);
            };

            MediaEngine.InitSize += (sender, args) =>
            {
                _rcSrc = args.NewVideSize;
                _nativeAspectRatio = args.NativeAspectRatio;

                if (args.Initial)
                {
                    var matrix = this.GetTargetMatrix();
                    var newVideoSize = (Size)matrix.Transform((Vector)new Size(args.NewVideSize.cx, args.NewVideSize.cy));

                    OnInitSize(newVideoSize);
                }

                ResizeNormal(args.Initial);

                if (args.InvalidateSuggested)
                    InvalidateMediaWindow();
            };
        }
  
        private void CreateMediaWindow()
        {
            var window = Application.Current.MainWindow;
            var helper = new WindowInteropHelper(window);

            var matrix = window.GetDeviceMatrix();
            var windowLocation = matrix.Transform(new Point(window.Left, window.Top));

            var transformToWindow = TransformToAncestor(window);
            var relativeLocation = transformToWindow.Transform(new Point(0, 0));

            var screenLocation = new Point(windowLocation.X + relativeLocation.X, windowLocation.Y + relativeLocation.Y);
         //   var screenSize = (Size)matrix.Transform((Vector)new Size(ActualWidth, ActualHeight));

            _mediaWindow = new MediaWindow(helper.Handle, (int)Math.Ceiling(screenLocation.X), (int)Math.Ceiling(screenLocation.Y), 
                /*(int)Math.Floor(screenSize.Width)*/ 0, /*(int)Math.Floor(screenSize.Height)*/ 0,
                WindowsManagement.WS_VISIBLE | WindowsManagement.WS_POPUP);
            _mediaWindow.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
        }
  
        #region Public properties and methods

        public Brush LogoBrush
        {
            get { return (Brush)GetValue(LogoBrushProperty); }
            set { SetValue(LogoBrushProperty, value); }
        }
  
        public double LogoMaxWidth
        {
            get { return (double)GetValue(LogoMaxWidthProperty); }
            set { SetValue(LogoMaxWidthProperty, value); }
        }
  
        public double LogoMaxHeight
        {
            get { return (double)GetValue(LogoMaxHeightProperty); }
            set { SetValue(LogoMaxHeightProperty, value); }
        }

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
  
        public event MWContextMenuEventHandler MWContextMenu
        {
            add { AddHandler(MWContextMenuEvent, value); }
            remove { RemoveHandler(MWContextMenuEvent, value); }
        }
  
        public event RoutedEventHandler MWDoubleClick
        {
            add { AddHandler(MWDoubleClickEvent, value); }
            remove { RemoveHandler(MWDoubleClickEvent, value); }
        }
  
        public event RoutedEventHandler MWClick
        {
            add { AddHandler(MWClickEvent, value); }
            remove { RemoveHandler(MWClickEvent, value); }
        }
  
        public event RoutedEventHandler MWMouseEnter
        {
            add { AddHandler(MWMouseEnterEvent, value); }
            remove { RemoveHandler(MWMouseEnterEvent, value); }
        }
  
        public event RoutedEventHandler MWMouseLeave
        {
            add { AddHandler(MWMouseLeaveEvent, value); }
            remove { RemoveHandler(MWMouseLeaveEvent, value); }
        }

        public event MWMouseMoveEventHandler MWMouseMove
        {
            add { AddHandler(MWMouseMoveEvent, value); }
            remove { RemoveHandler(MWMouseMoveEvent, value); }
        }

        /// <summary>
        /// Occurs when new video has been rendered and indicates that
        /// the application should resize to accomodate the new video frame.
        /// </summary>
        public event InitSizeEventHandler InitSize
        {
            add { AddHandler(InitSizeEvent, value); }
            remove { RemoveHandler(InitSizeEvent, value); }
        }
  
        #endregion
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _border = Template.FindName("PART_Border", this) as Border;
            SetMediaWindowState(false);
        }
        
        private void SetMediaWindowState(bool active)
        {
            if (active)
            {
                _hwndHost = new MediaWindowHwndHost();
                _hwndHost.MessageHook += new System.Windows.Interop.HwndSourceHook(_hwndHost_MessageHook);
                if (_border != null)
                    _border.Child = _hwndHost;
            }
            else
            {
                if (_border != null)
                {
                    var rect = new Rectangle { StrokeThickness = 0.0 };

                    var binding = new Binding("LogoBrush") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(Shape.FillProperty, binding);

                    binding = new Binding("LogoMaxWidth") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(FrameworkElement.MaxWidthProperty, binding);

                    binding = new Binding("LogoMaxHeight") { Source = this, Mode = BindingMode.OneWay };
                    rect.SetBinding(FrameworkElement.MaxHeightProperty, binding);

                    _border.Child = rect;
                }

                if (_hwndHost != null)
                {
                    _hwndHost.MessageHook -= new System.Windows.Interop.HwndSourceHook(_hwndHost_MessageHook);
                    _hwndHost.Dispose();
                    _hwndHost = null;
                }
            }
        }

        private void InvalidateMediaWindow()
        {
            if (_mediaWindow != null)
                _mediaWindow.Invalidate();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (_mediaWindow != null)
            {
                ResizeNormal();
            }
        }

        private IntPtr _hwndHost_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            if (msg == (int)WindowsMessages.WM_SIZE)
            {
                ResizeNormal();

                handled = true;
            }

            return IntPtr.Zero;
        }

        private void _mediaWindow_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (_mwHandler != null)
                _mwHandler.HandleMessage(e.HWnd, e.Msg, e.WParam, e.LParam);
        }

        IMediaWindow IMediaWindowHost.GetMediaWindow()
        {
            if (_mediaWindow == null)
            {
                CreateMediaWindow();
                SetMediaWindowState(true);
            }

            return _mediaWindow;
        }

        protected IMediaEngine MediaEngine { get; private set; }

        protected virtual void OnInitSize(Size newVideoSize)
        {
            var args = new InitSizeEventArgs(newVideoSize);
            args.RoutedEvent = InitSizeEvent;
            RaiseEvent(args);
        }

        #region Resizing stuff
        private void ResizeNormal(bool initial = false)
        {
            if (!initial && MediaEngine.GraphState == GraphState.Reset)
                return;

            GDI.RECT rect;
            WindowsManagement.GetWindowRect(_hwndHost.Handle, out rect);
            int clientWidth = rect.right - rect.left;
            int clientHeight = rect.bottom - rect.top;

            double w = clientWidth;
            double h = clientHeight;
            double ratio = w / h;
            double dAspectRatio;

            _rcDest = rect;

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
                case AspectRatio.AR_1x1:
                    dAspectRatio = 1.0;
                    break;
                case AspectRatio.AR_5x4:
                    dAspectRatio = 5.0 / 4.0;
                    break;
                case AspectRatio.AR_16x10:
                    dAspectRatio = 16.0 / 10.0;
                    break;
                default:
                    {
                        // free aspect ratio
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
                    vert = (_rcSrc.cy * fixedSize / _divideSize) - clientHeight;
                    _rcDest.top += (vert >= 0) ? 0 : -vert / 2;
                    _rcDest.bottom = (vert >= 0) ? _rcDest.bottom : _rcDest.top + (_rcSrc.cy * fixedSize / _divideSize);
                    h = _rcDest.bottom - _rcDest.top;
                    w = h * dAspectRatio;
                    hor = clientWidth - (int)w;
                    _rcDest.left += (hor <= 0) ? 0 : hor / 2;
                    _rcDest.right = _rcDest.left + (int)w;
                }
                else
                {
                    hor = (_rcSrc.cx * fixedSize / _divideSize) - clientWidth;
                    // hor>=0 - client area is smaller than video hor size
                    _rcDest.left += (hor >= 0) ? 0 : -hor / 2;
                    _rcDest.right = (hor >= 0) ? _rcDest.right : _rcDest.left + (_rcSrc.cx * fixedSize / _divideSize);
                    w = _rcDest.right - _rcDest.left;
                    h = w / dAspectRatio;
                    vert = clientHeight - (int)h;
                    _rcDest.top += (vert <= 0) ? 0 : vert / 2;
                    _rcDest.bottom = _rcDest.top + (int)h;
                }

            }
            else
            {
                if (ratio >= dAspectRatio)
                {
                    h = _rcDest.bottom - _rcDest.top;
                    w = h * dAspectRatio;
                    hor = clientWidth - (int)w;
                    _rcDest.left += (hor <= 0) ? 0 : hor / 2;
                    _rcDest.right = _rcDest.left + (int)w;
                }
                else
                {
                    w = _rcDest.right - _rcDest.left;
                    h = w / dAspectRatio;
                    vert = clientHeight - (int)h;
                    _rcDest.top += (vert <= 0) ? 0 : vert / 2;
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
            MediaEngine.SetVideoPosition(ref _rcDestMW);
        }
        #endregion

        private class MediaWindowHandler
        {
            private bool _doubleClick; // fix for extra mouse up message we want to discard
            private uint _previousMousePosition; // fix against spurious WM_MOUSEMOVE messages, see http://blogs.msdn.com/oldnewthing/archive/2003/10/01/55108.aspx#55109
            private readonly MediaWindowHost _mwh;
            private readonly IMediaWindowHost _host;

            private bool _mouseOnWindow;

            public MediaWindowHandler(MediaWindowHost mwh)
            {
                _mwh = mwh;
                _host = mwh;
            }

            public void HandleMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                switch (msg)
                {
                    case (uint)WindowsMessages.WM_LBUTTONDBLCLK:
                        _doubleClick = true;
                        _mwh.RaiseEvent(new RoutedEventArgs(MWDoubleClickEvent));
                        break;
                    case (uint)WindowsMessages.WM_CONTEXTMENU:
                        {
                            var args = CreateScreenPositionEventArgs();
                            args.RoutedEvent = MWContextMenuEvent;
                            _mwh.RaiseEvent(args);
                        }
                        break;
                    case (uint)WindowsMessages.WM_LBUTTONUP:
                        {
                            IMediaWindow mediaWindow = _host.GetMediaWindow();
                            if (mediaWindow != null && mediaWindow.Handle == hWnd && _mwh.MediaEngine.IsMenuOn)
                            {
                                uint mylParam = (uint)lParam;
                                uint x = mylParam & 0x0000FFFF;
                                uint y = mylParam & 0xFFFF0000;
                                y >>= 16;

                                GDI.POINT pt = new GDI.POINT();
                                pt.x = (int)x;
                                pt.y = (int)y;
                                _mwh.MediaEngine.ActivateDVDMenuButtonAtPosition(pt);
                            }

                            if (!_mwh.MediaEngine.IsMenuOn && !_doubleClick)
                                _mwh.RaiseEvent(new RoutedEventArgs(MWClickEvent));
                            _doubleClick = false;
                        }
                        break;
                    case (uint)WindowsMessages.WM_MOUSEMOVE:
                        if ((uint)lParam != _previousMousePosition) // mouse was actually moved as its position has changed
                        {
                            _previousMousePosition = (uint)lParam;

                            IMediaWindow mediaWindow = _host.GetMediaWindow();
                            if (mediaWindow != null && mediaWindow.Handle == hWnd && _mwh.MediaEngine.IsMenuOn)
                            {
                                uint mylParam = (uint)lParam;
                                uint x = mylParam & 0x0000FFFF;
                                uint y = mylParam & 0xFFFF0000;
                                y >>= 16;

                                GDI.POINT pt = new GDI.POINT();
                                pt.x = (int)x;
                                pt.y = (int)y;
                                _mwh.MediaEngine.SelectDVDMenuButtonAtPosition(pt);
                            }

                            if (!_mouseOnWindow)
                            {
                                WindowsManagement.TRACKMOUSEEVENT tme =
                                    new WindowsManagement.TRACKMOUSEEVENT();
                                tme.cbSize = Marshal.SizeOf(tme);
                                tme.dwFlags = WindowsManagement.TME_LEAVE;
                                tme.dwHoverTime = WindowsManagement.HOVER_DEFAULT;
                                tme.hwndTrack = hWnd;

                                WindowsManagement._TrackMouseEvent(ref tme);
                                _mouseOnWindow = true;
                                _mwh.RaiseEvent(new RoutedEventArgs(MWMouseEnterEvent));
                            }

                            var args = CreateScreenPositionEventArgs();
                            args.RoutedEvent = MWMouseMoveEvent;
                            _mwh.RaiseEvent(args);
                        }
                        break;
                    case (uint)WindowsMessages.WM_MOUSELEAVE:
                        _mouseOnWindow = false;
                        _mwh.RaiseEvent(new RoutedEventArgs(MWMouseLeaveEvent));
                        break;
                    case (uint)WindowsMessages.WM_KEYDOWN:
                        if (_mwh.MediaEngine.IsMenuOn)
                        {
                            var code = wParam.ToInt32();
                            Key? key = null;
                            switch(code)
                            {
                            	case 0x0D:
                                    key = Key.Enter;
                                    break;
                                case 0x25:
                                    key = Key.Left;
                                    break;
                                case 0x26:
                                    key = Key.Up;
                                    break;
                                case 0x27:
                                    key = Key.Right;
                                    break;
                                case 0x28:
                                    key = Key.Down;
                                    break;
                            }

                            if (key.HasValue)
                                HandleKey(key.Value);
                        }
                        break;
                }
            }
  
            public void HandleKey(Key key)
            {
                if (_mwh.MediaEngine.IsMenuOn)
                {
                    switch(key)
                    {
                        case Key.Enter:
                            _mwh.MediaEngine.ActivateSelectedDVDMenuButton();
                            break;
                        case Key.Left:
                            _mwh.MediaEngine.SelectDVDMenuButtonLeft();
                            break;
                        case Key.Right:
                            _mwh.MediaEngine.SelectDVDMenuButtonRight();
                            break;
                        case Key.Up:
                            _mwh.MediaEngine.SelectDVDMenuButtonUp();
                            break;
                        case Key.Down:
                            _mwh.MediaEngine.SelectDVDMenuButtonDown();
                            break;
                    }
                }
            }
  
            private ScreenPositionEventArgs CreateScreenPositionEventArgs()
            {
                GDI.POINT pt;
                NoCat.GetCursorPos(out pt);

                var matrix = _mwh.GetTargetMatrix();
                var screenPosition = matrix.Transform(new Point(pt.x, pt.y));

                return new ScreenPositionEventArgs(screenPosition);
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_mwHandler != null)
                _mwHandler.HandleKey(e.Key);
        }
    }
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Pvp.App.Messaging;
using Pvp.App.Util;
using res = Pvp.App.Resources;
using GalaSoft.MvvmLight.Messaging;

namespace Pvp.App.View
{
    [TemplatePart(Name = "PART_ResizeGrip_Width", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_ResizeGrip_Height", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_VideoArea", Type = typeof(Border))]
    public partial class MainWindow : Window, IMediaControlAcceptor
    {
        static MainWindow()
        {
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(MainWindow)));
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen",
                                        typeof(bool),
                                        typeof(MainWindow),
                                        new PropertyMetadata(false, new PropertyChangedCallback(IsFullScreenChanged)));
        
        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized",
                                        typeof(bool),
                                        typeof(MainWindow),
                                        new PropertyMetadata(false, new PropertyChangedCallback(IsMaximizedChanged)));

        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register("IsMinimized",
                                        typeof(bool),
                                        typeof(MainWindow),
                                        new PropertyMetadata(false, new PropertyChangedCallback(IsMinimizedChanged)));

        public static readonly DependencyProperty StartupLocationProperty =
            DependencyProperty.Register("StartupLocation", typeof(WindowStartupLocation), typeof(MainWindow), 
            new PropertyMetadata(default(WindowStartupLocation), new PropertyChangedCallback(StartupLocationChanged)));

        public static readonly DependencyProperty MWEventSourceProperty =
            DependencyProperty.Register("MWEventSource", typeof(object), typeof(MainWindow), new PropertyMetadata(default(object)));

        private enum WindowResizeMode
        {
            None,
            Width,
            Height
        }

        private const int EXTRA_GAP = 5;
        private WindowResizeMode _resizeMode;
        private Rect _restoreBounds;
        private Rect? _preFullScreenBounds;

        public MainWindow()
        {
            InitializeComponent();

            _resizeMode = WindowResizeMode.None;

            Messenger.Default.Register<CommandMessage>(this, OnCommand);
            Messenger.Default.Register<ResizeMainWindowCommandMessage>(this, OnResizeMainWindowCommand);

            Binding binding = new Binding("IsMaximized");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(IsMaximizedProperty, binding);

            binding = new Binding("IsMinimized");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(IsMinimizedProperty, binding);

            binding = new Binding("IsFullScreen");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(IsFullScreenProperty, binding);

            binding = new Binding("TopMost");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(TopmostProperty, binding);

            binding = new Binding("CenterWindow");
            binding.Source = DataContext;
            binding.Mode = BindingMode.OneWay;
            binding.Converter = new BooleanToWindowStartupLocationValueConverter();
            this.SetBinding(StartupLocationProperty, binding);
        }

        private static void IsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (MainWindow)d;

            var helper = new WindowInteropHelper(win);
            if (helper.Handle == IntPtr.Zero)
            {
                win._fullScreenChangePending = (bool)e.NewValue;
                return;
            }

            SetFullScreen(win, (bool)e.NewValue);
        }

        private static void SetFullScreen(MainWindow win, bool isFullScreen)
        {
            if (isFullScreen)
            {
                win._preFullScreenBounds = win.RestoreBounds;

                win.MoveWindow(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }
            else
            {
                if (win._preFullScreenBounds.HasValue)
                {
                    var bounds = win._preFullScreenBounds.Value;
                    win.MoveWindow(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                }
            }
        }

        private bool? _fullScreenChangePending;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (_fullScreenChangePending.HasValue)
            {
                SetFullScreen(this, _fullScreenChangePending.Value);
            }
        }

        private static void IsMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (MainWindow)d;
            if ((bool)e.NewValue)
            {
                win._restoreBounds = win.RestoreBounds;

                var workArea = SystemParameters.WorkArea;
                win.MoveWindow(0, 0, workArea.Width, workArea.Height);
            }
            else
            {
                var bounds = win._restoreBounds;
                win.MoveWindow(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            }

            win.SetMaximizeButtonTooltip();
        }

        private static void IsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (MainWindow)d;
            if ((bool)e.NewValue)
            {
                win.WindowState = WindowState.Minimized;
            }
            else
            {
                win.WindowState = WindowState.Normal;
            }
        }

        private static void StartupLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (MainWindow)d;
            var location = (WindowStartupLocation)e.NewValue;

            win.WindowStartupLocation = location;
        }

        public WindowStartupLocation StartupLocation
        {
            get { return (WindowStartupLocation)GetValue(StartupLocationProperty); }
            set { SetValue(StartupLocationProperty, value); }
        }

        public bool IsFullScreen
        {
            get
            {
                return (bool)GetValue(IsFullScreenProperty);
            }
            set
            {
                SetValue(IsFullScreenProperty, value);
            }
        }

        public bool IsMaximized
        {
            get
            {
                return (bool)GetValue(IsMaximizedProperty);
            }
            set
            {
                SetValue(IsMaximizedProperty, value);
            }
        }

        public bool IsMinimized
        {
            get
            {
                return (bool)GetValue(IsMinimizedProperty);
            }
            set
            {
                SetValue(IsMinimizedProperty, value);
            }
        }

        public object MWEventSource
        {
            get { return (object)GetValue(MWEventSourceProperty); }
            set { SetValue(MWEventSourceProperty, value); }
        }

        private void OnCommand(CommandMessage message)
        {
            if (message.Content == Command.ApplicationClose)
            {
                Close();
            }
        }

        private void OnResizeMainWindowCommand(ResizeMainWindowCommandMessage resizeMessage)
        {
            if (!IsMaximized && !IsMinimized && !IsFullScreen)
            {
                ResizeMainWindow(resizeMessage);
            }
        }

        private void ResizeMainWindow(ResizeMainWindowCommandMessage resizeMessage)
        {
            var left = Left;
            var top = Top;
            var width = ActualWidth;
            var height = ActualHeight;
            var move = false;

            if (resizeMessage.Size != null && resizeMessage.Coefficient.HasValue)
            {
                var mediaControlSize = _mainView.MediaControlSize;

                var hor = resizeMessage.Size.Item1 * resizeMessage.Coefficient.Value - mediaControlSize.Width;
                var ver = resizeMessage.Size.Item2 * resizeMessage.Coefficient.Value - mediaControlSize.Height;

                width += hor;
                height += ver;

                move = true;
            }

            if (resizeMessage.CenterWindow)
            {
                var workArea = SystemParameters.WorkArea;

                left = width < workArea.Width ? (workArea.Width - width) / 2 : workArea.Left;
                top = height < workArea.Height ? (workArea.Height - height) / 2 : workArea.Top;

                move = true;
            }

            if (move)
            {
                this.MoveWindow(left, top, width, height);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            Messenger.Default.Send(new EventMessage(Event.MainWindowClosing));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var grip = Template.FindName("PART_ResizeGrip_Width", this) as UIElement;
            if (grip != null)
            {
                grip.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    StartResize((UIElement)sender, WindowResizeMode.Width);
                };

                grip.MouseLeftButtonUp += new MouseButtonEventHandler(EndResize);
                grip.MouseMove += new MouseEventHandler(Resize);
            }

            grip = Template.FindName("PART_ResizeGrip_Height", this) as UIElement;
            if (grip != null)
            {
                grip.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    StartResize((UIElement)sender, WindowResizeMode.Height);
                };

                grip.MouseLeftButtonUp += new MouseButtonEventHandler(EndResize);
                grip.MouseMove += new MouseEventHandler(Resize);
            }

            var titleBar = Template.FindName("PART_TitleBar", this) as UIElement;
            if (titleBar != null)
            {
                titleBar.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    if (e.ClickCount == 2)
                    {
                        Messenger.Default.Send(new EventMessage(Event.TitleBarDoubleClick));
                    }
                    else if (!IsMaximized && !IsFullScreen)
                    {
                        DragMove();
                    }
                };
            }

            var button = Template.FindName("PART_MinimizeButton", this) as Button;
            if (button != null)
            {
                Binding binding = new Binding("MinimizeCommand");
                binding.Source = DataContext;
                binding.Mode = BindingMode.OneWay;
                button.SetBinding(ButtonBase.CommandProperty, binding);
            }

            button = Template.FindName("PART_MaximizeButton", this) as Button;
            if (button != null)
            {
                Binding binding = new Binding("MaximizeCommand");
                binding.Source = DataContext;
                binding.Mode = BindingMode.OneWay;
                button.SetBinding(ButtonBase.CommandProperty, binding);
            }

            button = Template.FindName("PART_CloseButton", this) as Button;
            if (button != null)
            {
                Binding binding = new Binding("CloseCommand");
                binding.Source = DataContext;
                binding.Mode = BindingMode.OneWay;
                button.SetBinding(ButtonBase.CommandProperty, binding);
            }

            var videoArea = Template.FindName("PART_VideoArea", this) as Border;
            if (videoArea != null)
            {
                videoArea.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    if (e.ClickCount == 2)
                    {
                        Messenger.Default.Send(new EventMessage(Event.VideoAreaDoubleClick));
                    }
                };
            }

            var icon = Template.FindName("PART_AppIcon_Pixels", this) as Border;
            if (icon != null)
            {
                double width, height;
                Extensions.TransformFromPixels((int)icon.Width, (int)icon.Height, out width, out height);
                icon.Width = width;
                icon.Height = height;
            }

            UpdateTooltips();
        }

        private void Resize(object sender, MouseEventArgs e)
        {
            if (_resizeMode != WindowResizeMode.None)
            {
                switch (_resizeMode)
                {
                    case WindowResizeMode.Width:
                        double width = e.GetPosition(this).X + EXTRA_GAP;
                        if (width > 0)
                            this.Width = width;
                        break;
                    case WindowResizeMode.Height:
                        double height = e.GetPosition(this).Y + EXTRA_GAP;
                        if (height > 0)
                            this.Height = height;
                        break;
                }
            }
        }

        private void StartResize(UIElement element, WindowResizeMode resizeMode)
        {
            if (element.CaptureMouse())
            {
                _resizeMode = resizeMode;
            }
        }

        private void EndResize(object sender, MouseButtonEventArgs e)
        {
            _resizeMode = WindowResizeMode.None;

            var element = (UIElement)sender;
            element.ReleaseMouseCapture();
        }

        private void SetMaximizeButtonTooltip()
        {
            var button = Template.FindName("PART_MaximizeButton", this) as Button;
            if (button != null)
            {
                button.ToolTip = IsMaximized ? res.Resources.captionbar_restore : res.Resources.captionbar_maximize;
            }
        }

        private void UpdateTooltips()
        {
            SetMaximizeButtonTooltip();

            var button = Template.FindName("PART_MinimizeButton", this) as Button;
            if (button != null)
            {
                button.ToolTip = res.Resources.captionbar_minimize;
            }

            button = Template.FindName("PART_CloseButton", this) as Button;
            if (button != null)
            {
                button.ToolTip = res.Resources.captionbar_close;
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            var filenames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filenames != null && filenames.Any())
            {
                Messenger.Default.Send(new DragDropMessage(filenames.First()));
            }
        }

        Core.Wpf.MediaControl IMediaControlAcceptor.MediaControl
        {
            set { MWEventSource = value; }
        }
    }
}

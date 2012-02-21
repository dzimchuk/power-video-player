using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Dzimchuk.Pvp.App.Messaging;
using Dzimchuk.Pvp.App.Util;
using res = Dzimchuk.Pvp.App.Resources;
using GalaSoft.MvvmLight.Messaging;

namespace Dzimchuk.Pvp.App.View
{
    [TemplatePart(Name = "PART_ResizeGrip_Width", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_ResizeGrip_Height", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_VideoArea", Type = typeof(Border))]
    public partial class MainWindow : Window
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

        private enum WindowResizeMode
        {
            None,
            Width,
            Height
        }

        private const int EXTRA_GAP = 5;
        private WindowResizeMode _resizeMode;
        private Rect _restoreBounds;
        private Rect _preFullScreenBounds;

        public MainWindow()
        {
            InitializeComponent();

            _resizeMode = WindowResizeMode.None;

            Messenger.Default.Register<CommandMessage>(this, OnCommand);

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
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            double x, y;
            this.TransformFromPixels(16, 16, out x, out y);
            Console.WriteLine();
        }

        private static void IsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (MainWindow)d;
            if ((bool)e.NewValue)
            {
                win._preFullScreenBounds = win.RestoreBounds;

                win.MoveWindow(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }
            else
            {
                var bounds = win._preFullScreenBounds;
                win.MoveWindow(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
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

        private void OnCommand(CommandMessage message)
        {
            if (message.Content == Command.ApplicationClose)
                Close();
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
                        Messenger.Default.Send(new EventMessage(Event.TitleBarDoubleClick), MessageTokens.UI);
                    }
                    else
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
                        Messenger.Default.Send(new EventMessage(Event.VideoAreaDoubleClick), MessageTokens.UI);
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
    }
}

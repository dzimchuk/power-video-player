using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Pvp.App.Composition;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.Wpf;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private readonly DispatcherTimer _timer;

        public MainView()
        {
            InitializeComponent();

            var acceptors = DependencyResolver.Current.ResolveAll<IMediaControlAcceptor>();
            foreach (var acceptor in acceptors)
            {
                acceptor.MediaControl = _mediaControl;
            }

            Messenger.Default.Send<EventMessage>(new EventMessage(Event.MediaControlCreated));

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Messenger.Default.Send(new EventMessage(Event.DispatcherTimerTick));
        }

        private void _mediaControl_MWContextMenu(object sender, ScreenPositionEventArgs args)
        {
            if (_mediaControl.ContextMenu != null)
            {
                _mediaControl.ContextMenu.PlacementTarget = _mediaControl;
                _mediaControl.ContextMenu.IsOpen = true;
            }
        }
  
        private void _mediaControl_MWDoubleClick(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(new EventMessage(Event.VideoAreaDoubleClick));
        }
  
        public void _mediaControl_MWMouseMove(object sender, ScreenPositionEventArgs e)
        {
            if (_fullScreenPanelHolder.IsOpen)
            {
                if (e.ScreenPosition.Y >= ActualHeight - _fullScreenControlPanel.DesiredSize.Height)
                {
                    _fullScreenControlPanel.OnMouseEnter();
                }
            }
        }

        private CustomPopupPlacement[] fullscreenPlacementCallback(
            Size popupSize,
            Size targetSize,
            Point offset)
        {
            var position = new Point(offset.X, targetSize.Height - popupSize.Height + offset.Y);
            return new CustomPopupPlacement[] { new CustomPopupPlacement(position, PopupPrimaryAxis.None) };
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_fullScreenPanelHolder.IsOpen)
            {
                var element = _fullScreenControlPanel.InputHitTest(e.GetPosition(_fullScreenControlPanel));
                if (element != null)
                {
                    _fullScreenControlPanel.OnMouseEnter();
                }
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(new EventMessage(Event.ContextMenuOpened));
        }

        private void _mediaControl_InitSize(object sender, InitSizeEventArgs arg)
        {
            Messenger.Default.Send(new EventMessage(Event.InitSize, arg));
        }

        public Size MediaControlSize
        {
            get { return new Size(_mediaControl.ActualWidth, _mediaControl.ActualHeight); }
        }
    }
}

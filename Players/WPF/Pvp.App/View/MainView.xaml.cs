using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public MainView()
        {
            InitializeComponent();

            var acceptor = (IMediaControlAcceptor)DependencyResolver.Current.Resolve<IMediaControlAcceptor>();
            acceptor.MediaControl = _mediaControl;

            Messenger.Default.Send<EventMessage>(new EventMessage(Event.MediaControlCreated), MessageTokens.App);
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
            Messenger.Default.Send(new EventMessage(Event.VideoAreaDoubleClick), MessageTokens.UI);
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
    }
}

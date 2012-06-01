using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pvp.App.Composition;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

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

        private void _mediaControl_MWContextMenu(object sender, Core.Wpf.MWContextMenuEventArgs args)
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
    }
}

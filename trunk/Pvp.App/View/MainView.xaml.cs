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

            var provider = (IMediaEngineProviderSetter)DependencyResolver.Current.Resolve<IMediaEngineProviderSetter>();
            provider.MediaEngine = _mediaWindowHost.MediaEngine;

            Messenger.Default.Send<EventMessage>(new EventMessage(Event.MediaWindowHostCreated), MessageTokens.App);
        }
    }
}

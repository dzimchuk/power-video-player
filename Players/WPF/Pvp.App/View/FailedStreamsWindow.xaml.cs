using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for FailedStreamsWindow.xaml
    /// </summary>
    public partial class FailedStreamsWindow : Window
    {
        public FailedStreamsWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this,
                                                       message =>
                                                           {
                                                               if (message.Content == Command.FailedStreamsWindowClose)
                                                               {
                                                                   Close();
                                                               }
                                                           });
        }
    }
}
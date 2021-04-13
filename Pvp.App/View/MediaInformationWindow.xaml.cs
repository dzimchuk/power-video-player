using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for MediaInformationWindow.xaml
    /// </summary>
    public partial class MediaInformationWindow : Window
    {
        public MediaInformationWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this,
                                                       message =>
                                                       {
                                                           if (message.Content == Command.MediaInformationWindowClose)
                                                           {
                                                               Close();
                                                           }
                                                       });
        }
    }
}
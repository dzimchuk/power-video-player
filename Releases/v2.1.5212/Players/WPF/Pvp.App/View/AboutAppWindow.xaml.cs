using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for AboutAppWindow.xaml
    /// </summary>
    public partial class AboutAppWindow : Window
    {
        public AboutAppWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this,
                                                       message =>
                                                           {
                                                               if (message.Content == Command.AboutAppWindowClose)
                                                               {
                                                                   Close();
                                                               }
                                                           });
        }
    }
}
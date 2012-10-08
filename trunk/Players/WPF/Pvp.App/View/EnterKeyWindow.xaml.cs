using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for EnterKeyWindow.xaml
    /// </summary>
    public partial class EnterKeyWindow : Window
    {
        public EnterKeyWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this, OnCommand);
        }

        private void OnCommand(CommandMessage message)
        {
            if (message.Content == Command.EnterKeyWindowClose)
            {
                Close();
            }
        }
    }
}
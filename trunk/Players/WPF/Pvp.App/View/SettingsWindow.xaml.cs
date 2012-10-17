using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<CommandMessage>(this,
                message =>
                {
                    if (message.Content == Command.SettingsWindowClose)
                    {
                    	Close();
                    }
                });
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var width = _btnOk.ActualWidth;
            if (_btnCancel.ActualWidth > width)
                width = _btnCancel.ActualWidth;
            if (_btnApply.ActualWidth > width)
                width = _btnApply.ActualWidth;

            _btnApply.Width = width;
            _btnCancel.Width = width;
            _btnOk.Width = width;
        }

        private void _listKeys_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var keyCombinationItem = (KeyCombinationItem)_listKeys.SelectedItem;
            Messenger.Default.Send(new EventMessage(Event.EditKeyCombination, new EditKeyCombinationEventArgs(keyCombinationItem)));
        }
    }
}

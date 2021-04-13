using System;
using System.Linq;
using System.Windows;
using Pvp.App.View;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class DialogService : IDialogService
    {
        private SettingsWindow _settingsDalog;

        public void DisplaySettingsDialog()
        {
            _settingsDalog = new SettingsWindow();
            _settingsDalog.Owner = Application.Current.MainWindow;
            _settingsDalog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _settingsDalog.ShowDialog();
            _settingsDalog = null;

            Application.Current.MainWindow.Activate();
        }

        public KeyCombination ShowEnterKeyWindow()
        {
            var dlg = new EnterKeyWindow();
            dlg.Owner = _settingsDalog ?? Application.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dlg.ShowDialog();
            return dlg.SelectedKeyCombination;
        }

        private static void ShowModalDialog(Window dlg)
        {
            dlg.Owner = Application.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dlg.ShowDialog();
            Application.Current.MainWindow.Activate();
        }

        public void DisplayMediaInformationWindow()
        {
            var dlg = new MediaInformationWindow();
            ShowModalDialog(dlg);
        }

        public void DisplayFailedStreamsWindow()
        {
            var dlg = new FailedStreamsWindow();
            ShowModalDialog(dlg);
        }

        public void DisplayAboutAppWindow()
        {
            var dlg = new AboutAppWindow();
            ShowModalDialog(dlg);
        }

        public void DisplayMessage(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DisplayWarning(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public void DisplayError(string message)
        {
            MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        public bool DisplayYesNoDialog(string message)
        {
            return MessageBox.Show(message, Resources.Resources.program_name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}

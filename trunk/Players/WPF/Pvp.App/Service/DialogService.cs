using System;
using System.Linq;
using System.Windows;
using Pvp.App.View;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class DialogService : IDialogService
    {
        public void DisplaySettingsDialog()
        {
            var dlg = new SettingsWindow();
            dlg.Owner = Application.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dlg.ShowDialog();
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

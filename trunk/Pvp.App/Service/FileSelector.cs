using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pvp.App.ViewModel;
using System.Windows;

namespace Pvp.App.Service
{
    internal class FileSelector : IFileSelector
    {
        public string SelectFile(string filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = filter;
            dlg.Multiselect = false;

            var result = dlg.ShowDialog(Application.Current.MainWindow);
            return result.HasValue && result.Value == true && !string.IsNullOrEmpty(dlg.FileName) ? dlg.FileName : null;
        }
    }
}

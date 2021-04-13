using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Pvp.App.ViewModel;

namespace Pvp.App.Service
{
    internal class FileSelector : IFileSelector
    {
        public string SelectFile(string filter)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = filter;
            dlg.Multiselect = false;

            var result = dlg.ShowDialog(Application.Current.MainWindow);
            return result.HasValue && result.Value && !string.IsNullOrEmpty(dlg.FileName) ? dlg.FileName : null;
        }

        public string SelectFolder(string defaultFolder)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
//            dlg.Description = Resources.Resources.ResourceManager.GetString("folder_select_dialog_caption");
//            dlg.UseDescriptionForTitle = true;
            if (!string.IsNullOrEmpty(defaultFolder) && Directory.Exists(defaultFolder))
            {
                dlg.SelectedPath = defaultFolder;
            }
            dlg.ShowNewFolderButton = true;

            var result = dlg.ShowDialog(Application.Current.MainWindow);
            return result.HasValue && result.Value && !string.IsNullOrEmpty(dlg.SelectedPath) ? dlg.SelectedPath : null;
        }
    }
}
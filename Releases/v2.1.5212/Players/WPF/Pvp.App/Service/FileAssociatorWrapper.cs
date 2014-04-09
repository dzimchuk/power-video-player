using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.App.Util.FileTypes;
using Pvp.App.ViewModel.Settings;
using IFileAssociator = Pvp.App.ViewModel.Settings.IFileAssociator;

namespace Pvp.App.Service
{
    internal class FileAssociatorWrapper : IFileAssociator, IFileAssociatorRegistration
    {
        private const string ProgramName = "Power Video Player";
        private const string DocTypePrefix = "PVP.AssocFile";

        public void SetStatus(IEnumerable<FileTypeItem> items)
        {
            using (var fa = FileAssociatorFactory.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                foreach (var item in items)
                {
                    item.Selected = fa.IsAssociated(Normalize(item.Extension));
                }
            }
        }

        public void Associate(IEnumerable<FileTypeItem> items)
        {
            using (var fa = FileAssociatorFactory.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                foreach (var item in items)
                {
                    var ext = Normalize(item.Extension);
                    if (item.Selected)
                    {
                        fa.Associate(ext);
                    }
                    else
                    {
                        fa.UnAssociate(ext);
                    }
                }

                fa.NotifyShell();
            }
        }

        public bool CanAssociate
        {
            get
            {
                using (var fa = FileAssociatorFactory.GetFileAssociator(DocTypePrefix, ProgramName))
                {
                    return fa.CanAssociate;
                }
            }
        }

        public bool CanShowExternalUI
        {
            get
            {
                using (var fa = FileAssociatorFactory.GetFileAssociator(DocTypePrefix, ProgramName))
                {
                    return fa.CanShowExternalUI;
                }
            }
        }

        public bool ShowExternalUI()
        {
            using (var fa = FileAssociatorFactory.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                return fa.ShowExternalUI();
            }
        }

        private string Normalize(string extension)
        {
            return extension.StartsWith(".") ? extension : string.Format(".{0}", extension);
        }

        public void Register(string regPath,
                             string defaultIcon,
                             string openCommand,
                             string localizedAppName,
                             string localizedAppDescription,
                             IEnumerable<DocTypeInfo> docTypes)
        {
            using (var fa = FileAssociatorFactory.GetAppRegisterer(DocTypePrefix, ProgramName))
            {
                fa.Register(regPath, defaultIcon, openCommand, localizedAppName, localizedAppDescription, docTypes);
            }
        }

        public void Unregister()
        {
            using (var fa = FileAssociatorFactory.GetAppRegisterer(DocTypePrefix, ProgramName))
            {
                fa.Unregister();
            }
        }
    }
}
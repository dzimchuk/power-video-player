using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.App.Util;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.Service
{
    internal class FileAssociatorWrapper : IFileAssociator, IFileAssociatorRegistration
    {
        private const string ProgramName = "Power Video Player";
        private const string DocTypePrefix = "PVP.AssocFile";

        public void SetStatus(IEnumerable<FileTypeItem> items)
        {
            using (FileAssociator fa = FileAssociator.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                foreach (var item in items)
                {
                    item.Selected = fa.IsAssociated(Normalize(item.Extension));
                }
            }
        }

        public void Associate(IEnumerable<FileTypeItem> items)
        {
            using (FileAssociator fa = FileAssociator.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                foreach (var item in items)
                {
                    fa.Associate(Normalize(item.Extension), item.Selected);
                }
            }

            FileAssociator.NotifyShell();
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
                             IEnumerable<FileAssociator.DocTypeInfo> docTypes)
        {
            using (FileAssociator fa = FileAssociator.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                fa.Register(regPath, defaultIcon, openCommand, localizedAppName, localizedAppDescription, docTypes);
            }
        }

        public void Unregister()
        {
            using (FileAssociator fa = FileAssociator.GetFileAssociator(DocTypePrefix, ProgramName))
            {
                fa.Unregister();
            }
        }
    }
}
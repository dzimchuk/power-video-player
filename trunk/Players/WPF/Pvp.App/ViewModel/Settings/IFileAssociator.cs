using System;
using System.Collections.Generic;
using Pvp.App.Util;

namespace Pvp.App.ViewModel.Settings
{
    public interface IFileAssociator
    {
        void SetStatus(IEnumerable<FileTypeItem> items);
        void Associate(IEnumerable<FileTypeItem> items);

        void Register(string regPath,
                      string defaultIcon,
                      string openCommand,
                      string localizedAppName,
                      string localizedAppDescription,
                      IEnumerable<FileAssociator.DocTypeInfo> docTypes);

        void Unregister();
    }
}
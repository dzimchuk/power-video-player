using System;
using System.Collections.Generic;

namespace Pvp.App.ViewModel.Settings
{
    public interface IFileAssociator
    {
        void SetStatus(IEnumerable<FileTypeItem> items);
        void Associate(IEnumerable<FileTypeItem> items);
    }
}
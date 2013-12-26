using System;
using Pvp.Core.Native;

namespace Pvp.App.Util.FileTypes
{
    internal class FileAssociator : IFileAssociator
    {
        private readonly string _docTypePrefix;
        private readonly string _appName;

        public FileAssociator(string docTypePrefix, string appName)
        {
            _docTypePrefix = docTypePrefix;
            _appName = appName;
        }

        public string DocTypePrefix
        {
            get { return _docTypePrefix; }
        }

        public string AppName
        {
            get { return _appName; }
        }

        public bool CanAssociate { get { return true; } }

        public bool IsAssociated(string ext)
        {
            var docType = RegistryHelper.GetClassesValueUser(ext, null, string.Empty);
            return (docType != null && docType == (_docTypePrefix + ext.ToUpperInvariant()));
        }

        public void Associate(string ext)
        {
            var docType = _docTypePrefix + ext.ToUpperInvariant();
            var prevDocType = RegistryHelper.GetClassesValueUser(ext, null, string.Empty);
            if (prevDocType != null && prevDocType != docType)
                RegistryHelper.SetClassesValueUser(ext, "PrevDefault", prevDocType);
            RegistryHelper.SetClassesValueUser(ext, null, docType);
        }

        public void UnAssociate(string ext)
        {
            if (IsAssociated(ext))
            {
                var prevDocType = RegistryHelper.GetClassesValueUser(ext, "PrevDefault", string.Empty);
                if (prevDocType != null)
                    RegistryHelper.SetClassesValueUser(ext, null, prevDocType);
                else
                    RegistryHelper.SetClassesValueUser(ext, null, string.Empty);
            }
        }

        public void NotifyShell()
        {
            Shell.SHChangeNotify(Shell.SHCNE_ASSOCCHANGED, Shell.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public void Dispose()
        {
        }
    }
}
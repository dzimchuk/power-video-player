using System;

namespace Pvp.App.Util.FileTypes
{
    public interface IFileAssociator : IDisposable
    {
        bool CanAssociate { get; }
        string DocTypePrefix { get; }
        string AppName { get; }

        bool IsAssociated(string ext);
        void Associate(string ext);
        void UnAssociate(string ext);
        void NotifyShell();
    }
}
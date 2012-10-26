using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.App.Util;

namespace Pvp.App
{
    public interface IFileAssociatorRegistration
    {
        void Register(string regPath,
                      string defaultIcon,
                      string openCommand,
                      string localizedAppName,
                      string localizedAppDescription,
                      IEnumerable<FileAssociator.DocTypeInfo> docTypes);

        void Unregister();
    }
}
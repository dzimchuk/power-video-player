using System;

namespace Pvp.App.Util.FileTypes
{
    public static class FileAssociatorFactory
    {
        public static IFileAssociator GetFileAssociator(string docTypePrefix, string appName)
        {
            if ((Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2) || Environment.OSVersion.Version.Major > 6)
                return new DefaultProgramsFileAssociator8(docTypePrefix, appName);
            else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2)
                return new DefaultProgramsFileAssociator(docTypePrefix, appName);
            else
                return new FileAssociator(docTypePrefix, appName);
        }

        public static AppRegisterer GetAppRegisterer(string docTypePrefix, string appName)
        {
            return new AppRegisterer(GetFileAssociator(docTypePrefix, appName));
        }
    }
}
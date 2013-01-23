using System;
using System.Reflection;
using GalaSoft.MvvmLight;

namespace Pvp.App.ViewModel
{
    public class AboutAppViewModel : ViewModelBase
    {
        public string ProgramNameAndVersion
        {
            get { return string.Format("{0} {1}", Resources.Resources.program_name, GetProductVersion()); }
        }

        public string CopyRight
        {
            get { return Resources.Resources.about_copyright; }
        }

        public string License
        {
            get { return Resources.Resources.about_license; }
        }

        private string GetProductVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (infoVersion != null)
                return infoVersion.InformationalVersion;

            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (fileVersion != null)
                return fileVersion.Version;

            var version = assembly.GetName().Version;
            return FormatVersion(version.Major, version.Minor, version.Build);
        }

        private string FormatVersion(int major, int minor, int build)
        {
            return string.Format("{0}.{1}.{2}", major, minor, build);
        }
    }
}
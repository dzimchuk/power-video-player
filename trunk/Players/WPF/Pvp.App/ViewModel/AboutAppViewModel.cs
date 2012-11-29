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
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
    }
}
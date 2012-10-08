using GalaSoft.MvvmLight;
using Pvp.App.Composition;

namespace Pvp.App
{
    internal static class ViewModelLocator
    {
        internal const string MainViewModelName = "MainViewModel";
        internal const string MainWindowViewModelName = "MainWindowViewModel";
        internal const string SettingsViewModelName = "SettingsViewModelName";
        internal const string EnterKeyViewModelName = "EnterKeyViewModelName";

        static ViewModelLocator()
        {
            DesignTimeComposition.SetUpDependencies();
        }

        public static ViewModelBase MainViewModel
        {
            get
            {
                return (ViewModelBase)DependencyResolver.Current.Resolve<ViewModelBase>(MainViewModelName);
            }
        }

        public static ViewModelBase MainWindowViewModel
        {
            get
            {
                return (ViewModelBase)DependencyResolver.Current.Resolve<ViewModelBase>(MainWindowViewModelName);
            }
        }

        public static ViewModelBase SettingsViewModel
        {
            get { return (ViewModelBase)DependencyResolver.Current.Resolve<ViewModelBase>(SettingsViewModelName); }
        }

        public static ViewModelBase EnterKeyViewModel
        {
            get { return (ViewModelBase)DependencyResolver.Current.Resolve<ViewModelBase>(EnterKeyViewModelName); }
        }
    }
}

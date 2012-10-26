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
        internal const string MediaInformationViewModelName = "MediaInformationViewModelName";
        internal const string FailedStreamsViewModelName = "FailedStreamsViewModelName";
        internal const string AboutAppViewModelName = "AboutAppViewModelName";

        static ViewModelLocator()
        {
            DesignTimeComposition.SetUpDependencies();
        }

        public static ViewModelBase MainViewModel
        {
            get
            {
                return DependencyResolver.Current.Resolve<ViewModelBase>(MainViewModelName);
            }
        }

        public static ViewModelBase MainWindowViewModel
        {
            get
            {
                return DependencyResolver.Current.Resolve<ViewModelBase>(MainWindowViewModelName);
            }
        }

        public static ViewModelBase SettingsViewModel
        {
            get { return DependencyResolver.Current.Resolve<ViewModelBase>(SettingsViewModelName); }
        }

        public static ViewModelBase EnterKeyViewModel
        {
            get { return DependencyResolver.Current.Resolve<ViewModelBase>(EnterKeyViewModelName); }
        }

        public static ViewModelBase MediaInformationViewModel
        {
            get { return DependencyResolver.Current.Resolve<ViewModelBase>(MediaInformationViewModelName); }
        }

        public static ViewModelBase FailedStreamsViewModel
        {
            get { return DependencyResolver.Current.Resolve<ViewModelBase>(FailedStreamsViewModelName); }
        }

        public static ViewModelBase AboutAppViewModel
        {
            get { return DependencyResolver.Current.Resolve<ViewModelBase>(AboutAppViewModelName); }
        }
    }
}

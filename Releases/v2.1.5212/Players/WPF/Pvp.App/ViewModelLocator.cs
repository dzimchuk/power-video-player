using Pvp.App.Composition;
using Pvp.App.ViewModel;
using Pvp.App.ViewModel.MainView;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App
{
    internal static class ViewModelLocator
    {
        static ViewModelLocator()
        {
            DesignTimeComposition.SetUpDependencies();
        }

        public static MainWindowViewModel MainWindowViewModel
        {
            get { return DependencyResolver.Current.Resolve<MainWindowViewModel>(); }
        }

        public static MainViewModel MainViewModel
        {
            get { return DependencyResolver.Current.Resolve<MainViewModel>(); }
        }

        public static SubpictureMenuViewModel SubpictureMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<SubpictureMenuViewModel>(); }
        }

        public static AudioMenuViewModel AudioMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<AudioMenuViewModel>(); }
        }

        public static FiltersMenuViewModel FiltersMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<FiltersMenuViewModel>(); }
        }

        public static DvdMenuViewModel DvdMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<DvdMenuViewModel>(); }
        }

        public static DvdMenuLanguagesViewModel DvdMenuLanguagesViewModel
        {
            get { return DependencyResolver.Current.Resolve<DvdMenuLanguagesViewModel>(); }
        }

        public static DvdAnglesMenuViewModel DvdAnglesMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<DvdAnglesMenuViewModel>(); }
        }

        public static DvdChaptersMenuViewModel DvdChaptersMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<DvdChaptersMenuViewModel>(); }
        }

        public static DiscMenuViewModel DiscMenuViewModel
        {
            get { return DependencyResolver.Current.Resolve<DiscMenuViewModel>(); }
        }

        public static SettingsViewModel SettingsViewModel
        {
            get { return DependencyResolver.Current.Resolve<SettingsViewModel>(); }
        }

        public static EnterKeyViewModel EnterKeyViewModel
        {
            get { return DependencyResolver.Current.Resolve<EnterKeyViewModel>(); }
        }

        public static MediaInformationViewModel MediaInformationViewModel
        {
            get { return DependencyResolver.Current.Resolve<MediaInformationViewModel>(); }
        }

        public static FailedStreamsViewModel FailedStreamsViewModel
        {
            get { return DependencyResolver.Current.Resolve<FailedStreamsViewModel>(); }
        }

        public static AboutAppViewModel AboutAppViewModel
        {
            get { return DependencyResolver.Current.Resolve<AboutAppViewModel>(); }
        }
    }
}

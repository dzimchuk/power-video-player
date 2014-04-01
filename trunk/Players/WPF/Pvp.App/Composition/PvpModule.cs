using System;
using System.Linq;
using System.Windows;
using Ninject.Modules;
using Pvp.App.Service;
using Pvp.App.ViewModel;
using Pvp.App.ViewModel.MainView;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.Composition
{
    public class PvpModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMediaEngineFacade>().ToMethod(context => MediaEngineFacade.Instance);
            Bind<IMediaControlAcceptor>().ToMethod(context => MediaEngineFacade.Instance);
            Bind<IMediaControlAcceptor>().ToMethod(context => (IMediaControlAcceptor)Application.Current.MainWindow);

            Bind<IFileSelector>().To<FileSelector>();
            Bind<IDialogService>().To<DialogService>();
            Bind<IWindowHandleProvider>().To<WindowHandleProvider>();
            Bind<IDriveService>().To<DriveService>();
            Bind<ISettingsProvider>().To<SettingsProvider>().InSingletonScope();
            Bind<IKeyInterpreter>().To<KeyInterpreter>();
            Bind<IMouseWheelInterpreter>().To<MouseWheelInterpreter>();
            Bind<ISelectedKeyCombinationItemResolver>().To<SelectedKeyCombinationItemResolver>();
            Bind<IFileAssociator>().To<FileAssociatorWrapper>();
            Bind<IFileAssociatorRegistration>().To<FileAssociatorWrapper>();
            Bind<IDisplayService>().To<DisplayService>();
            Bind<IFailedStreamsContainer>().To<FailedStreamsContainer>().InSingletonScope();
            Bind<ICursorManager>().To<CursorManager>();

            Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
            Bind<MainViewModel>().ToSelf().InSingletonScope();
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();

            Bind<SubpictureMenuViewModel>().ToSelf().InSingletonScope();
            Bind<AudioMenuViewModel>().ToSelf().InSingletonScope();
            Bind<FiltersMenuViewModel>().ToSelf().InSingletonScope();
            Bind<DvdMenuViewModel>().ToSelf().InSingletonScope();
            Bind<DvdMenuLanguagesViewModel>().ToSelf().InSingletonScope();
            Bind<DvdAnglesMenuViewModel>().ToSelf().InSingletonScope();
            Bind<DvdChaptersMenuViewModel>().ToSelf().InSingletonScope();

            Bind<IImageCreaterFactory>().To<ImageCreaterFactory>().InSingletonScope();

            Bind<SettingsViewModel>().ToSelf();
            Bind<EnterKeyViewModel>().ToSelf();
            Bind<MediaInformationViewModel>().ToSelf();
            Bind<FailedStreamsViewModel>().ToSelf();
            Bind<AboutAppViewModel>().ToSelf();
        }
    }
}

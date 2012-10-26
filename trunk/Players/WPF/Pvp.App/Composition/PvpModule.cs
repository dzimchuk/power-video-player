using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using Ninject.Modules;
using Pvp.App.Service;
using Pvp.App.ViewModel;
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

            Bind<ViewModelBase>().To<MainWindowViewModel>().InSingletonScope().Named(ViewModelLocator.MainWindowViewModelName);
            Bind<ViewModelBase>().To<MainViewModel>().InSingletonScope().Named(ViewModelLocator.MainViewModelName);
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();

            Bind<IImageCreaterFactory>().To<ImageCreaterFactory>().InSingletonScope();

            Bind<ViewModelBase>().To<SettingsViewModel>().Named(ViewModelLocator.SettingsViewModelName);
            Bind<ViewModelBase>().To<EnterKeyViewModel>().Named(ViewModelLocator.EnterKeyViewModelName);
            Bind<ViewModelBase>().To<MediaInformationViewModel>().Named(ViewModelLocator.MediaInformationViewModelName);
            Bind<ViewModelBase>().To<FailedStreamsViewModel>().Named(ViewModelLocator.FailedStreamsViewModelName);
            Bind<ViewModelBase>().To<AboutAppViewModel>().Named(ViewModelLocator.AboutAppViewModelName);
        }
    }
}

using System;
using System.Linq;
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

            Bind<IFileSelector>().To<FileSelector>();
            Bind<IDialogService>().To<DialogService>();
            Bind<IWindowHandleProvider>().To<WindowHandleProvider>();
            Bind<IDriveService>().To<DriveService>();
            Bind<ISettingsProvider>().To<SettingsProvider>().InSingletonScope();

            Bind<ViewModelBase>().To<MainWindowViewModel>().InSingletonScope().Named(ViewModelLocator.MainWindowViewModelName);
            Bind<ViewModelBase>().To<MainViewModel>().InSingletonScope().Named(ViewModelLocator.MainViewModelName);
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();

            Bind<ViewModelBase>().To<SettingsViewModel>().Named(ViewModelLocator.SettingsViewModelName);
        }
    }
}

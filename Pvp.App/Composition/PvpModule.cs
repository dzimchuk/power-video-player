using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Pvp.Core.MediaEngine;
using GalaSoft.MvvmLight;
using Pvp.App.ViewModel;
using Pvp.App.Service;

namespace Pvp.App.Composition
{
    public class PvpModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMediaEngineProvider>().ToMethod(context => MediaEngineProvider.Instance);
            Bind<IMediaEngineProviderSetter>().ToMethod(context => MediaEngineProvider.Instance);

            Bind<IFileSelector>().To<FileSelector>();
            Bind<IDialogService>().To<DialogService>();

            Bind<ViewModelBase>().To<MainWindowViewModel>().InSingletonScope().Named(ViewModelLocator.MainWindowViewModelName);
            Bind<ViewModelBase>().To<MainViewModel>().InSingletonScope().Named(ViewModelLocator.MainViewModelName);
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();
        }
    }
}

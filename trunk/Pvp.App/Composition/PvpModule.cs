using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Dzimchuk.MediaEngine.Core;
using GalaSoft.MvvmLight;
using Dzimchuk.Pvp.App.ViewModel;
using Dzimchuk.Pvp.App.Service;

namespace Dzimchuk.Pvp.App.Composition
{
    public class PvpModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMediaEngineProvider>().ToMethod(context => MediaEngineProvider.Instance);
            Bind<IMediaEngineProviderSetter>().ToMethod(context => MediaEngineProvider.Instance);

            Bind<IFileSelector>().To<FileSelector>();

            Bind<ViewModelBase>().To<MainWindowViewModel>().InSingletonScope().Named(ViewModelLocator.MainWindowViewModelName);
            Bind<ViewModelBase>().To<MainViewModel>().InSingletonScope().Named(ViewModelLocator.MainViewModelName);
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();
        }
    }
}

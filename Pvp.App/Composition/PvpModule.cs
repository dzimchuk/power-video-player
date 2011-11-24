using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Dzimchuk.MediaEngine.Core;
using GalaSoft.MvvmLight;
using Dzimchuk.Pvp.App.ViewModel;

namespace Dzimchuk.Pvp.App.Composition
{
    public class PvpModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMediaEngine>().ToMethod(context => MediaEngineServiceProvider.GetMediaEngine()).InSingletonScope();

            Bind<ViewModelBase>().To<MainViewModel>().InSingletonScope().Named(ViewModelLocator.MainViewModelName);
            Bind<ControlPanelViewModel>().ToSelf().InSingletonScope();
        }
    }
}

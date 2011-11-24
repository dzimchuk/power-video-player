using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dzimchuk.Pvp.App.Composition;
using GalaSoft.MvvmLight;

namespace Dzimchuk.Pvp.App
{
    internal class ViewModelLocator
    {
        internal const string MainViewModelName = "MainViewModel";

        public static ViewModelBase MainViewModel
        {
            get
            {
                return (ViewModelBase)DependencyResolver.Current.Resolve<ViewModelBase>(MainViewModelName);
            }
        }
    }
}

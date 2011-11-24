using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Dzimchuk.MediaEngine.Core;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace Dzimchuk.Pvp.App.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly IMediaEngine _engine;
        private readonly ControlPanelViewModel _controlViewModel;
        
        public MainViewModel(IMediaEngine engine, ControlPanelViewModel controlViewModel)
        {
            _engine = engine;
            _controlViewModel = controlViewModel;
        }

        public ControlPanelViewModel ControlViewModel
        {
            get { return _controlViewModel; }
        }

        
    }
}

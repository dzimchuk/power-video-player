using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using Pvp.App.Util;

namespace Pvp.App.ViewModel
{
    /// <summary>
    /// This class will help hook up a workaround for context menu (see Pvp.App.Util.BoundCommand).
    /// NOTE: paramter must be castable from NULL because RelayCommand<T> will cast it to T and glitchy
    /// context menu will send NULL's during the first pass.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GenericRelayCommand<T> : RelayCommand<T>, ICanExecuteChangedRaiser
    {
        public GenericRelayCommand(Action<T> execute)
            : base(execute, null)
        {
        }

        public GenericRelayCommand(Action<T> execute, Predicate<T> canExecute)
            : base(execute, canExecute)
        {
        }
    }
}

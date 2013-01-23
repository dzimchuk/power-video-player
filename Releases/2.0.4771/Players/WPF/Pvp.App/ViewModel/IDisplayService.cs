using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    public interface IDisplayService
    {
        void PreventMonitorPowerdown();
        void AllowMonitorPowerdown();
    }
}
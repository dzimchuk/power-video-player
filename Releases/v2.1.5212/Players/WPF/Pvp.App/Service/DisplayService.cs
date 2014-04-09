using System;
using System.Linq;
using Pvp.App.ViewModel;
using Pvp.Core.Native;

namespace Pvp.App.Service
{
    internal class DisplayService : IDisplayService
    {
        public void PreventMonitorPowerdown()
        {
            NoCat.SetThreadExecutionState(NoCat.EXECUTION_STATE.ES_DISPLAY_REQUIRED | NoCat.EXECUTION_STATE.ES_CONTINUOUS);
        }

        public void AllowMonitorPowerdown()
        {
            NoCat.SetThreadExecutionState(NoCat.EXECUTION_STATE.ES_CONTINUOUS);
        }
    }
}
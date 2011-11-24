using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dzimchuk.Pvp.App.Controls
{
    public class FullScreenControlPanel : ControlPanelBase
    {
        static FullScreenControlPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FullScreenControlPanel), new FrameworkPropertyMetadata(typeof(FullScreenControlPanel)));
        }
    }
}

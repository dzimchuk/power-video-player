using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Dzimchuk.Pvp.App.Controls
{
    public class RegularControlPanel : ControlPanelBase
    {
        static RegularControlPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegularControlPanel), new FrameworkPropertyMetadata(typeof(RegularControlPanel)));
        }
    }
}

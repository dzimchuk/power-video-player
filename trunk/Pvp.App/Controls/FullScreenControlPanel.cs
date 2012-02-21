using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dzimchuk.Pvp.App.Controls
{
    [TemplateVisualState(Name = "MouseEnter", GroupName = "MouseStates")]
    [TemplateVisualState(Name = "MouseLeave", GroupName = "MouseStates")]
    public class FullScreenControlPanel : ControlPanelBase
    {
        static FullScreenControlPanel()
        {
            EventManager.RegisterClassHandler(typeof(FullScreenControlPanel), UIElement.MouseEnterEvent, new RoutedEventHandler(OnMouseEnter), true);
            EventManager.RegisterClassHandler(typeof(FullScreenControlPanel), UIElement.MouseLeaveEvent, new RoutedEventHandler(OnMouseLeave), true);
        }

        private static void OnMouseEnter(Object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "MouseEnter", true);
        }

        private static void OnMouseLeave(Object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "MouseLeave", true);
        }
    }
}

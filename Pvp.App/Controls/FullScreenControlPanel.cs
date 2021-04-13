using System;
using System.Linq;
using System.Windows;

namespace Pvp.App.Controls
{
    [TemplateVisualState(Name = "MouseEnter", GroupName = "MouseStates")]
    [TemplateVisualState(Name = "MouseLeave", GroupName = "MouseStates")]
    public class FullScreenControlPanel : ControlPanelBase
    {
        private static bool _tracking;

        static FullScreenControlPanel()
        {
            EventManager.RegisterClassHandler(typeof(FullScreenControlPanel), UIElement.MouseEnterEvent, new RoutedEventHandler(OnMouseEnter), true);
            EventManager.RegisterClassHandler(typeof(FullScreenControlPanel), UIElement.MouseLeaveEvent, new RoutedEventHandler(OnMouseLeave), true);
        }

        private static void OnMouseEnter(Object sender, RoutedEventArgs e)
        {
            OnMouseEnter(sender);
        }
  
        private static void OnMouseLeave(Object sender, RoutedEventArgs e)
        {
            if (_tracking)
            {
                VisualStateManager.GoToState((FrameworkElement)sender, "MouseLeave", true);
                _tracking = false;
            }
        }
  
        private static void OnMouseEnter(object sender)
        {
            if (!_tracking)
            {
                VisualStateManager.GoToState((FrameworkElement)sender, "MouseEnter", true);
                _tracking = true;
            }
        }

        public void OnMouseEnter()
        {
            FullScreenControlPanel.OnMouseEnter(this);
        }
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Pvp.Core.Native;

namespace Pvp.App.Util
{
    public static class WindowCustomizer
    {
        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.RegisterAttached("ShowIcon", typeof(bool), typeof(WindowCustomizer),
                                                new PropertyMetadata(true, OnShowIconChanged));

        private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as Window;
            if (win == null)
                return;

            var helper = new WindowInteropHelper(win);
            var hwnd = helper.EnsureHandle();

            int extendedStyle = WindowsManagement.GetWindowLong(hwnd, WindowsManagement.GWL_EXSTYLE);
            WindowsManagement.SetWindowLong(hwnd, WindowsManagement.GWL_EXSTYLE, extendedStyle ^ WindowsManagement.WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            WindowsManagement.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, WindowsManagement.SWP_NOMOVE |
                                                                          WindowsManagement.SWP_NOSIZE | WindowsManagement.SWP_NOZORDER |
                                                                          WindowsManagement.SWP_FRAMECHANGED);
        }

        public static void SetShowIcon(UIElement element, bool value)
        {
            element.SetValue(ShowIconProperty, value);
        }

        public static bool GetShowIcon(UIElement element)
        {
            return (bool)element.GetValue(ShowIconProperty);
        }
    }
}
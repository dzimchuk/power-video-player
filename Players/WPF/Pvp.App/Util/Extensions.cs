using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using Pvp.Core.Native;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Pvp.App.Util
{
    internal static class Extensions
    {
        /// <summary>
        /// Transforms device independent units (1/96 of an inch)
        /// to pixels
        /// </summary>
        /// <param name="visual">a visual object</param>
        /// <param name="unitX">a device independent unit value X</param>
        /// <param name="unitY">a device independent unit value Y</param>
        /// <param name="pixelX">returns the X value in pixels</param>
        /// <param name="pixelY">returns the Y value in pixels</param>
        public static void TransformToPixels(this Visual visual,
                                             double unitX,
                                             double unitY,
                                             out int pixelX,
                                             out int pixelY)
        {
            Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
            pixelX = (int)(matrix.M11 * unitX);
            pixelY = (int)(matrix.M22 * unitY);
        }

        /// <summary>
        /// Transforms pixels
        /// to device independent units (1/96 of an inch)
        /// </summary>
        /// <param name="visual">a visual object</param>
        /// <param name="unitX">X value in pixels</param>
        /// <param name="unitY">Y value in pixels</param>
        /// <param name="pixelX">returns the device independent unit value X</param>
        /// <param name="pixelY">returns the device independent unit value Y</param>
        public static void TransformFromPixels(this Visual visual,
                                               int pixelX,
                                               int pixelY,
                                               out double unitX,
                                               out double unitY)
        {
            Matrix matrix = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
            unitX = pixelX * matrix.M11;
            unitY = pixelY * matrix.M22;
        }

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;

        /// <summary>
        /// Transforms pixels
        /// to device independent units (1/96 of an inch)
        /// </summary>
        /// <param name="unitX">X value in pixels</param>
        /// <param name="unitY">Y value in pixels</param>
        /// <param name="pixelX">returns the device independent unit value X</param>
        /// <param name="pixelY">returns the device independent unit value Y</param>
        public static void TransformFromPixels(int pixelX,
                                               int pixelY,
                                               out double unitX,
                                               out double unitY)
        {
            IntPtr hDc = GetDC(IntPtr.Zero);
            if (hDc != IntPtr.Zero)
            {
                int dpiX = GetDeviceCaps(hDc, LOGPIXELSX);
                int dpiY = GetDeviceCaps(hDc, LOGPIXELSY);
                ReleaseDC(IntPtr.Zero, hDc);

                unitX = ((double)96 / dpiX) * pixelX;
                unitY = ((double)96 / dpiY) * pixelY;
            }
            else
                throw new ArgumentNullException("Failed to get DC.");
        }

        public static void MoveWindow(this Window window,
                                      double left,
                                      double top,
                                      double width,
                                      double height)
        {
            int pxLeft = 0, pxTop = 0;
            if (left != 0 || top != 0)
                window.TransformToPixels(left, top, out pxLeft, out pxTop);

            int pxWidth, pxHeight;
            window.TransformToPixels(width, height, out pxWidth, out pxHeight);

            var helper = new WindowInteropHelper(window);
            WindowsManagement.MoveWindow(helper.Handle, pxLeft, pxTop, pxWidth, pxHeight, true);
        }
    }
}

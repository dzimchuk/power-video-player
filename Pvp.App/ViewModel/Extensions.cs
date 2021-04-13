using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using Pvp.App.Messaging;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel
{
    internal static class Extensions
    {
        public static double GetResizeCoefficient(this VideoSize size)
        {
            switch (size)
            {
                case VideoSize.SIZE_FREE:
                    throw new InvalidOperationException(size.ToString());
                case VideoSize.SIZE100:
                    return 1.0;
                case VideoSize.SIZE200:
                    return 2.0;
                case VideoSize.SIZE50:
                    return 0.5;
                default:
                    throw new NotSupportedException(size.ToString());
            }
        }

        public static void RaiseResizeMainWindowEvent(this VideoSize size, Tuple<double, double> videoSize, bool centerWindow)
        {
            var resizeMessage = size == VideoSize.SIZE_FREE ?
                        new ResizeMainWindowCommandMessage(Command.ResizeMainWindow, centerWindow)
                        :
                        new ResizeMainWindowCommandMessage(Command.ResizeMainWindow, videoSize, size.GetResizeCoefficient(), centerWindow);
            Messenger.Default.Send(resizeMessage);
        }
    }
}
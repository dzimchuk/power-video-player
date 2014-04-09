using System;
using Pvp.App.ViewModel;
using Pvp.Core.MediaEngine;
using Pvp.Core.Wpf;

namespace Pvp.App.Service
{
    internal class ImageCreaterFactory : IImageCreaterFactory
    {
        public IImageCreator GetNew()
        {
            return new ImageCreator();
        }
    }
}
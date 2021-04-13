using System;
using System.Linq;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel
{
    public interface IImageCreaterFactory
    {
        IImageCreator GetNew();
    }
}
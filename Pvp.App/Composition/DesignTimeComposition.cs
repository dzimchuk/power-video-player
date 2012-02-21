using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using System.ComponentModel;
using System.Windows;

namespace Dzimchuk.Pvp.App.Composition
{
    internal static class DesignTimeComposition
    {
        internal static void SetUpDependencies()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var kernel = new StandardKernel();
                kernel.Load(typeof(App).Assembly);

                var resolver = new NinjectDependencyResolver(kernel);
                DependencyResolver.SetResolver(resolver);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Dzimchuk.Pvp.App.Composition
{
    internal class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object Resolve<T>()
        {
            return _kernel.Get<T>();
        }

        public object Resolve<T>(string name)
        {
            return _kernel.Get<T>(name);
        }
    }
}

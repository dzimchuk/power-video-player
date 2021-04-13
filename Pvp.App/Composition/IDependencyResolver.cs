using System;
using System.Collections.Generic;
using System.Linq;

namespace Pvp.App.Composition
{
    internal interface IDependencyResolver
    {
        T Resolve<T>();
        T Resolve<T>(string name);
        IEnumerable<T> ResolveAll<T>();
    }
}
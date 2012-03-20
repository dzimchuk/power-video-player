using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.App.Composition
{
    internal interface IDependencyResolver
    {
        object Resolve<T>();
        object Resolve<T>(string name);
    }
}

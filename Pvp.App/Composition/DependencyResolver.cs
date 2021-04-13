using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvp.App.Composition
{
    internal class DependencyResolver
    {
        private static readonly Lazy<DependencyResolver> _instance = new Lazy<DependencyResolver>(true);

        public static IDependencyResolver Current
        {
            get { return _instance.Value.Resolver; }
        }

        public static void SetResolver(IDependencyResolver resolver)
        {
            _instance.Value.Resolver = resolver;
        }

        private IDependencyResolver Resolver { get; set; }
    }
}

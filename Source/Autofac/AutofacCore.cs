using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace Core.Autofac
{
    public class AutoFacCore
    {
        private static readonly Lazy<ILifetimeScope> _core = new Lazy<ILifetimeScope>(() => CreateCore(Modules));

        private static readonly List<IModule> Modules = new List<IModule>();

        public static ILifetimeScope Init(params IModule[] modules)
        {
            Modules.Clear();
            Modules.AddRange(modules);
            return Core;
        }

        public static ILifetimeScope InitScope(params IModule[] modules)
        {
            return CreateCore(modules);
        }

        private static ILifetimeScope CreateCore(IEnumerable<IModule> modules)
        {
            var builder = new ContainerBuilder();
            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }

            return builder.Build().BeginLifetimeScope();
        }

        public static ILifetimeScope Core
        {
            get { return _core.Value; }
        }
    }
}

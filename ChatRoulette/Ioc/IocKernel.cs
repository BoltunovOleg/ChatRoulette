using System;
using ChatRoulette.Properties;
using Ninject;
using Ninject.Modules;
using Ninject.Syntax;

namespace ChatRoulette.Ioc
{
    public class IocKernel
    {
        private static IKernel _kernel;

        public static T Get<T>()
        {
            if (_kernel == null)
                throw new ArgumentNullException(nameof(_kernel), Resources.IocKernel_Get_IoC_ядро_не_инициализированно_);
            return _kernel.Get<T>();
        }

        public static IBindingToSyntax<T> Bind<T>()
        {
            if (_kernel == null)
                throw new ArgumentNullException(nameof(_kernel), Resources.IocKernel_Get_IoC_ядро_не_инициализированно_);
            return _kernel.Rebind<T>();
        }

        public static void Initialize(params INinjectModule[] modules)
        {
            if (_kernel != null) return;
            _kernel = new StandardKernel(modules);
        }
    }
}
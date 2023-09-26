///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using Castle.Windsor;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.container
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Creates the default service collection.
        /// </summary>
        /// <returns></returns>
        public static IContainer CreateDefaultContainer(bool initialize = true)
        {
            var wrapper = new ContainerImpl(new WindsorContainer());
            if (initialize) {
                wrapper.InitializeDefaultServices();
            }

            return wrapper;
        }

        public static void CheckContainer(this IContainer container)
        {
            if (container == null) {
                throw new ArgumentException(
                    "container is null, please initialize",
                    nameof(container));
            }
        }

        public static ILockManager LockManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<ILockManager>();
        }

        public static IReaderWriterLockManager RWLockManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IReaderWriterLockManager>();
        }

        public static IThreadLocalManager ThreadLocalManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IThreadLocalManager>();
        }

        public static IResourceManager ResourceManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IResourceManager>();
        }

        public static TypeResolverProvider TypeResolverProvider(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<TypeResolverProvider>();
        }

        public static TypeResolver TypeResolver(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<TypeResolver>();
        }

        private static bool TryCreateInstance<T>(this IContainer container, ConstructorInfo constructor, out T instanceValue)
        {
            container.CheckContainer();

            instanceValue = default(T);

            foreach (var parameter in constructor.GetParameters()) {
                if (!container.Has(parameter.ParameterType)) {
                    return false;
                }
            }

            var parameters = constructor.GetParameters()
                .Select(param => container.Resolve(param.ParameterType))
                .ToArray();

            instanceValue = (T) constructor.Invoke(parameters);

            return true;
        }

        public static T CreateInstance<T>(this IContainer container, Type viewFactoryClass)
        {
            container.CheckContainer();

            var constructor = viewFactoryClass.GetConstructor(
                new Type[] {typeof(IContainer)});
            if (constructor != null) {
                return (T) constructor.Invoke(new object[] { container });
            }

            constructor = viewFactoryClass.GetConstructor(Type.EmptyTypes);
            if (constructor != null) {
                return (T) constructor.Invoke(Array.Empty<object>());
            }

            var instance = default(T);
            var constructors = viewFactoryClass.GetConstructors();
            if (constructors.Any(ctor => TryCreateInstance(container, ctor, out instance))) {
                return instance;
            }

            throw new ArgumentException("unable to create an instance of type " + viewFactoryClass.FullName);
        }

        public static T ResolveSingleton<T>(
            this IContainer container,
            Supplier<T> instanceSupplier)
            where T : class
        {
            container.CheckContainer();

            lock (container) {
                if (container.Has<T>()) {
                    return container.Resolve<T>();
                }

                var instance = instanceSupplier.Invoke();
                container.Register<T>(instance, Lifespan.Singleton, null);
                return instance;
            }
        }
    }
}

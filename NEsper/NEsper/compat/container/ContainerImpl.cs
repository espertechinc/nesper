///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace com.espertech.esper.compat.container
{
    public class ContainerImpl : IContainer
    {
        private readonly Guid _id;
        private readonly IWindsorContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerImpl"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ContainerImpl(IWindsorContainer container)
        {
            _id = Guid.NewGuid();
            _container = container;
        }

        public IWindsorContainer WindsorContainer => _container;

        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Resolves a named object within a container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T Resolve<T>(string name)
        {
            return _container.Resolve<T>(name);
        }

        /// <summary>
        /// Resolves the specified arguments as anonymous type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentsAsAnonymousType">Type of the arguments as anonymous.</param>
        /// <returns></returns>
        public T Resolve<T>(object argumentsAsAnonymousType)
        {
            return _container.Resolve<T>(argumentsAsAnonymousType);
        }

        /// <summary>
        /// Resolves the specified arguments as dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentsAsDictionary">The arguments as dictionary.</param>
        /// <returns></returns>
        public T Resolve<T>(IDictionary argumentsAsDictionary)
        {
            return _container.Resolve<T>(argumentsAsDictionary);
        }

        /// <summary>
        /// Returns true if an object by the given name is registered.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [has] [the specified name]; otherwise, <c>false</c>.
        /// </returns>
        public bool Has(string name)
        {
            return _container.Kernel.HasComponent(name);
        }

        /// <summary>
        /// Returns true if an object by the given type is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>()
        {
            return _container.Kernel.HasComponent(typeof(T));
        }

        public bool Has(Type serviceType)
        {
            return _container.Kernel.HasComponent(serviceType);
        }

        public bool DoesNotHave<T>()
        {
            return !Has<T>();
        }

        // these methods are provided as a convenience ... we cannot abstract
        // away all facets of an IoC container, so we have taken a somewhat
        // opinionated view of the IoC container.  we may abstract away other
        // parts at a later time.

        public IContainer Register<T, TImpl>(Lifespan lifespan, string name) 
            where T : class
            where TImpl : T
        {
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().ImplementedBy<TImpl>(), name),
                    lifespan));
            return this;
        }

        public IContainer Register<T>(T value, Lifespan lifespan, string name)
            where T : class
        {
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().Instance(value), name),
                    lifespan));
            return this;
        }

        public IContainer Register<T>(Func<IContainer, T> factory, Lifespan lifespan, string name)
            where T : class
        {
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().UsingFactoryMethod(kernel => factory.Invoke(this)), name),
                    lifespan));
            return this;
        }

        private ComponentRegistration<T> WithName<T>(
            ComponentRegistration<T> componentRegistration,
            String name)
            where T : class
        {
            if (string.IsNullOrEmpty(name))
                return componentRegistration;

            return componentRegistration.Named(name);
        }

        private ComponentRegistration<T> WithLifespan<T>(
            ComponentRegistration<T> componentRegistration,
            Lifespan lifespan)
            where T : class
        {
            if (lifespan == Lifespan.Singleton) {
                return componentRegistration.LifestyleSingleton();
            } else if (lifespan == Lifespan.Transient) {
                return componentRegistration.LifeStyle.Transient;
            } else if (lifespan is Lifespan.LifespanTypeBound typeBound) {
                var method = componentRegistration.GetType().GetMethod("LifestyleBoundTo", new Type[] {});
                var genericMethod = method.MakeGenericMethod(typeBound.BoundType);
                return (ComponentRegistration<T>) genericMethod.Invoke(
                    componentRegistration, new object[0]);
            } else {
                throw new ArgumentException("invalid lifespan");
            }
        }
    }
}

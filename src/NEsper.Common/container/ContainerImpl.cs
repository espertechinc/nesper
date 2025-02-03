///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using Castle.MicroKernel.Registration;
using Castle.Windsor;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.container
{
    public class ContainerImpl : IContainer, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Guid _id = Guid.NewGuid();
        private IWindsorContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerImpl"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ContainerImpl(IWindsorContainer container)
        {
            _container = container;
        }

        public IWindsorContainer WindsorContainer => _container;

        /// <summary>
        /// Cleans up resources associated with this container.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            lock (this) {
                Log.Info("Dispose(): disposing of windsor container");
                _container?.Dispose();
                _container = null;
            }
        }

        public override string ToString()
        {
            return $"ContainerImpl:({nameof(_id)}: {_id}, {nameof(_container)}: {_container})";
        }

        private void CheckDisposed()
        {
            lock (this) {
                if (_container == null) {
                    throw new IllegalStateException("container " + _id + " has been disposed");
                }
            }
        }
        
        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        public object Resolve(Type serviceType)
        {
            CheckDisposed();
            return _container.Resolve(serviceType);
        }

        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            CheckDisposed();
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
            CheckDisposed();
            return _container.Resolve<T>(name);
        }

        /// <summary>
        /// Attempts to resolve a name within a container.  If the named entity does not exist, then it
        /// returns default(T).
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryResolve<T>(out T value)
        {
            lock (this)
            {
                CheckDisposed();
                if (Has<T>())
                {
                    value = Resolve<T>(); // should not throw any exceptions
                    return true;
                }
                
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a name within a container.  If the named entity does not exist, then it
        /// returns default(T).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryResolve<T>(string name, out T value)
        {
            lock (this)
            {
                CheckDisposed();
                if (Has<T>(name))
                {
                    value = Resolve<T>(name); // should not throw any exceptions
                    return true;
                }
                
                value = default(T);
                return false;
            }
        }
        
#if false
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
        public T Resolve<T>(IDictionary<object, object> argumentsAsDictionary)
        {
            return _container.Resolve<T>(argumentsAsDictionary);
        }
#endif
        
        /// <summary>
        /// Returns true if an object by the given name is registered.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if [has] [the specified name]; otherwise, <c>false</c>.
        /// </returns>
        public bool Has(string name)
        {
            CheckDisposed();
            return _container.Kernel.HasComponent(name);
        }

        /// <summary>
        /// Returns true if an object by the given type is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>()
        {
            CheckDisposed();
            return _container.Kernel.HasComponent(typeof(T));
        }

        public bool Has<T>(string name)
        {
            CheckDisposed();
            return _container.Kernel.HasComponent(name);
        }
        
        public bool Has(Type serviceType)
        {
            CheckDisposed();
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
            CheckDisposed();
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().ImplementedBy<TImpl>(), name),
                    lifespan));
            return this;
        }

        public IContainer Register<T>(T value, Lifespan lifespan, string name)
            where T : class
        {
            CheckDisposed();
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().Instance(value), name),
                    lifespan));
            return this;
        }

        public IContainer Register<T>(Func<IContainer, T> factory, Lifespan lifespan, string name)
            where T : class
        {
            CheckDisposed();
            _container.Register(
                WithLifespan(
                    WithName(Component.For<T>().UsingFactoryMethod(kernel => factory.Invoke(this)), name),
                    lifespan));
            return this;
        }

        private ComponentRegistration<T> WithName<T>(
            ComponentRegistration<T> componentRegistration,
            string name)
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
            if (lifespan == Lifespan.Singleton)
            {
                return componentRegistration.LifestyleSingleton();
            }
            else if (lifespan == Lifespan.Transient)
            {
                return componentRegistration.LifeStyle.Transient;
            }
            else if (lifespan is Lifespan.LifespanTypeBound typeBound)
            {
                var method = componentRegistration.GetType().GetMethod("LifestyleBoundTo", new Type[] { });
                var genericMethod = method.MakeGenericMethod(typeBound.BoundType);
                return (ComponentRegistration<T>)genericMethod.Invoke(
                    componentRegistration, Array.Empty<object>());
            }
            else
            {
                throw new ArgumentException("invalid lifespan");
            }
        }
    }
}
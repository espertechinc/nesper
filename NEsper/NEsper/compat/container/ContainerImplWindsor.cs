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
    public class ContainerImplWindsor : IContainer
    {
        private readonly IWindsorContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerImplWindsor"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ContainerImplWindsor(IWindsorContainer container)
        {
            _container = container;
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

        public void RegisterSingleton<T, TImpl>() 
            where T : class
            where TImpl : T
        {
            _container.Register(
                Component.For<T>().ImplementedBy<TImpl>().LifestyleSingleton());
        }

        public void RegisterSingleton<T>(T value)
            where T : class 
        {
            _container.Register(
                Component.For<T>().Instance(value).LifestyleSingleton());
        }

        public void RegisterSingleton<T>(Func<IContainer, T> factory)
            where T : class
        {
            _container.Register(
                Component.For<T>().UsingFactoryMethod(
                    kernel => factory.Invoke(this)));
        }
    }
}

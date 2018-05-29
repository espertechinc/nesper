///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace com.espertech.esper.compat.container
{
    public interface IContainer
    {
        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Resolves an object within a container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();

        /// <summary>
        /// Resolves a named object within a container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        T Resolve<T>(string name);

        /// <summary>
        /// Resolves the specified arguments as anonymous type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentsAsAnonymousType">Type of the arguments as anonymous.</param>
        /// <returns></returns>
        T Resolve<T>(object argumentsAsAnonymousType);

        /// <summary>
        /// Resolves the specified arguments as dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentsAsDictionary">The arguments as dictionary.</param>
        /// <returns></returns>
        T Resolve<T>(IDictionary argumentsAsDictionary);

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImpl">The type of the implementation.</typeparam>
        /// <param name="lifespan">The lifespan.</param>
        /// <param name="name">The name.</param>
        IContainer Register<T, TImpl>(Lifespan lifespan, string name = null)
            where T : class
            where TImpl : T;

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="lifespan">The lifespan.</param>
        /// <param name="name">The name.</param>
        IContainer Register<T>(T value, Lifespan lifespan, string name = null)
            where T : class;

        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <param name="lifespan">The lifespan.</param>
        /// <param name="name">The name.</param>
        IContainer Register<T>(Func<IContainer, T> factory, Lifespan lifespan, string name = null)
            where T : class;

        bool Has(string name);
        bool Has(Type serviceType);
        bool Has<T>();

        bool DoesNotHave<T>();
    }
}

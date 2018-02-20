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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();

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
        void RegisterSingleton<T, TImpl>()
            where T : class
            where TImpl : T;

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        void RegisterSingleton<T>(T value)
            where T : class;

        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        void RegisterSingleton<T>(Func<IContainer, T> factory)
            where T : class;

    }
}

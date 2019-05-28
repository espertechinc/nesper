///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util
{
    public class TransientConfigurationResolver
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TransientConfigurationResolver));

        public static ClassForNameProvider ResolveClassForNameProvider(
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve(
                transientConfiguration,
                ClassForNameProviderDefault.INSTANCE,
                ClassForNameProviderDefault.NAME,
                typeof(ClassForNameProvider));
        }

        public static ClassLoaderProvider ResolveClassLoader(
            IContainer container,
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve(
                transientConfiguration,
                container.Resolve<ClassLoaderProvider>(),
                ClassLoaderProviderDefault.NAME,
                typeof(ClassLoaderProvider));
        }

        private static T Resolve<T>(
            IDictionary<string, object> transientConfiguration,
            T defaultProvider,
            string name,
            Type interfaceClass)
        {
            if (transientConfiguration == null)
            {
                return defaultProvider;
            }

            var value = transientConfiguration.Get(name);
            if (value == null)
            {
                return defaultProvider;
            }

            if (!value.GetType().IsImplementsInterface(interfaceClass))
            {
                log.Warn(
                    "For transient configuration '" + name + "' expected an object implementing " +
                    interfaceClass.Name + " but received " + value.GetType() + ", using default provider");
                return defaultProvider;
            }

            return (T) value;
        }
    }
} // end of namespace
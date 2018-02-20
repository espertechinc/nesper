///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.util
{
    public class TransientConfigurationResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ClassForNameProvider ResolveClassForNameProvider(
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve<ClassForNameProvider>(
                transientConfiguration,
                ClassForNameProviderDefault.INSTANCE,
                ClassForNameProviderDefault.NAME,
                typeof (ClassForNameProvider));
        }

        public static FastClassClassLoaderProvider ResolveFastClassClassLoaderProvider(
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve<FastClassClassLoaderProvider>(
                transientConfiguration,
                FastClassClassLoaderProviderDefault.INSTANCE,
                FastClassClassLoaderProviderDefault.NAME,
                typeof (FastClassClassLoaderProvider));
        }

        public static ClassLoaderProvider ResolveClassLoader(
            ClassLoaderProvider classLoaderProvider,
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve<ClassLoaderProvider>(
                transientConfiguration, 
                classLoaderProvider,
                ClassLoaderProviderDefault.NAME,
                typeof (ClassLoaderProvider));
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
                Log.Warn(
                    "For transient configuration '" + name + "' expected an object implementing " + interfaceClass.Name +
                    " but received " + value.GetType() + ", using default provider");
                return defaultProvider;
            }
            return (T) value;
        }
    }
} // end of namespace
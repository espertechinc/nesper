///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.util
{
    public class TransientConfigurationResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TransientConfigurationResolver));

        public static TypeResolverProvider ResolveTypeResolverProvider(
            TypeResolverProvider defaultProvider,
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve(
                transientConfiguration,
                defaultProvider,
                TypeResolverProviderDefault.NAME,
                typeof(TypeResolverProvider));
        }

        public static TypeResolver ResolveTypeResolver(
            TypeResolverProvider defaultTypeResolverProvider,
            IDictionary<string, object> transientConfiguration,
            TypeResolver typeResolverDefault = null)
        {
            var typeResolver = Resolve(
                transientConfiguration,
                typeResolverDefault,
                TypeResolverConstants.NAME,
                typeof(TypeResolver));
            return typeResolver ?? ResolveTypeResolverProvider(defaultTypeResolverProvider, transientConfiguration).TypeResolver;
        }

        private static T Resolve<T>(
            IDictionary<string, object> transientConfiguration,
            T defaultProvider,
            string name,
            Type interfaceClass)
        {
            if (transientConfiguration == null) {
                return defaultProvider;
            }

            if (!transientConfiguration.TryGetValue(name, out var value)) {
                return defaultProvider;
            }

            if (!value.GetType().IsImplementsInterface(interfaceClass)) {
                Log.Warn(
                    $"For transient configuration '{name}' expected an object implementing {interfaceClass.Name} but received {value.GetType()}, using default provider");
                return defaultProvider;
            }

            return (T)value;
        }
    }
} // end of namespace
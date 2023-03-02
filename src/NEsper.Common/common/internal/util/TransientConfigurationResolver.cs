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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util
{
    public class TransientConfigurationResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TransientConfigurationResolver));

#if DEPRECATED
        public static ClassForNameProvider ResolveClassForNameProvider(
            IContainer container,
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve(
                transientConfiguration,
                container.Resolve<ClassForNameProvider>(),
                ClassForNameProviderDefault.NAME,
                typeof(ClassForNameProvider));
        }
#endif

        public static TypeResolverProvider ResolveTypeResolverProvider(
            IContainer container,
            IDictionary<string, object> transientConfiguration)
        {
            return Resolve(
                transientConfiguration,
                container.Resolve<TypeResolverProvider>(),
                TypeResolverProviderDefault.NAME,
                typeof(TypeResolverProvider));
        }
        
        public static TypeResolver ResolveTypeResolver(
            IContainer container,
            IDictionary<string, object> transientConfiguration,
            TypeResolver typeResolverDefault = null)
        {
            if (typeResolverDefault == null) {
                if (container.Has<TypeResolver>()) {
                    typeResolverDefault = container.Resolve<TypeResolver>();
                }
            }
            
            var typeResolver = Resolve(
                transientConfiguration,
                typeResolverDefault,
                TypeResolverConstants.NAME,
                typeof(TypeResolver));
            return typeResolver ?? ResolveTypeResolverProvider(container, transientConfiguration).GetTypeResolver();
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
                    "For transient configuration '" +
                    name +
                    "' expected an object implementing " +
                    interfaceClass.Name +
                    " but received " +
                    value.GetType() +
                    ", using default provider");
                return defaultProvider;
            }

            return (T) value;
        }
    }
} // end of namespace
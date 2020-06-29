///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.query;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

namespace com.espertech.esper.runtime.@internal.kernel.faf
{
    public class EPRuntimeHelperFAF
    {
        public static FAFProvider QueryMethod(
            EPCompiled compiled,
            EPServicesContext services)
        {
            var classLoader = services.ImportServiceRuntime.ClassLoader;
            
            //var classLoader = ClassProvidedImportClassLoaderFactory.GetClassLoader(
            //    compiled.Classes,
            //    services.ClassLoaderParent,
            //    services.ClassProvidedPathRegistry);
            
            var className = compiled.Manifest.QueryProviderClassName;

            // load module resource class
            Type clazz;
            try {
                clazz = classLoader.GetClass(className);
            }
            catch (TypeLoadException e) {
                throw new EPException(e);
            }

            // get FAF provider
            FAFProvider fafProvider;
            try {
                fafProvider = (FAFProvider) TypeHelper.Instantiate(clazz);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            if (classLoader is ClassProvidedImportClassLoader importClassLoader) {
                importClassLoader.Imported = fafProvider.ModuleDependencies.PathClasses;
            }

            // initialize event types
            IDictionary<string, EventType> moduleTypes = new Dictionary<string, EventType>();

            EventTypeResolverImpl eventTypeResolver = new EventTypeResolverImpl(
                moduleTypes,
                services.EventTypePathRegistry,
                services.EventTypeRepositoryBus,
                services.BeanEventTypeFactoryPrivate,
                services.EventSerdeFactory);

            EventTypeCollectorImpl eventTypeCollector = new EventTypeCollectorImpl(
                services.Container,
                moduleTypes,
                services.BeanEventTypeFactoryPrivate,
                classLoader,
                services.EventTypeFactory,
                services.BeanEventTypeStemService,
                eventTypeResolver,
                services.XmlFragmentEventTypeFactory,
                services.EventTypeAvroHandler,
                services.EventBeanTypedEventFactory,
                services.ImportServiceRuntime);

            fafProvider.InitializeEventTypes(new EPModuleEventTypeInitServicesImpl(eventTypeCollector, eventTypeResolver));

            // initialize query
            fafProvider.InitializeQuery(
                new EPStatementInitServicesImpl(
                    "faf-query",
                    EmptyDictionary<StatementProperty, object>.Instance, 
                    null,
                    null,
                    eventTypeResolver,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                    services));

            return fafProvider;
        }

        public static void ValidateSubstitutionParams(FAFQueryMethodProvider queryMethodProvider)
        {
            var substitutionParamsTypes = queryMethodProvider.QueryInformationals.SubstitutionParamsTypes;
            if (substitutionParamsTypes != null && substitutionParamsTypes.Length > 0) {
                throw new EPException("Missing values for substitution parameters, use prepare-parameterized instead");
            }
        }

        public static void CheckSubstitutionSatisfied(EPFireAndForgetPreparedQueryParameterizedImpl impl)
        {
            if (impl.UnsatisfiedParamsOneOffset.IsEmpty()) {
                return;
            }

            var num = impl.UnsatisfiedParamsOneOffset.First();
            if (impl.Names != null && !impl.Names.IsEmpty()) {
                string name = null;
                foreach (var entry in impl.Names) {
                    if (entry.Value == num) {
                        name = entry.Key;
                        break;
                    }
                }

                if (name != null) {
                    throw new EPException("Missing value for substitution parameter '" + name + "'");
                }
            }

            throw new EPException("Missing value for substitution parameter " + num);
        }
    }
} // end of namespace
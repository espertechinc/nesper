///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerFactoryHelper
    {
        public static ContextControllerFactory[] GetFactory(ContextControllerFactoryServiceContext serviceContext,
                                                            ContextStateCache contextStateCache)
        {
            if (!(serviceContext.Detail is ContextDetailNested))
            {
                ContextControllerFactory factory = BuildContextFactory(
                    serviceContext, serviceContext.ContextName, serviceContext.Detail, 1, null, contextStateCache);
                factory.ValidateFactory();
                return new ContextControllerFactory[]
                {
                    factory
                };
            }
            return BuildNestedContextFactories(serviceContext, contextStateCache);
        }

        private static ContextControllerFactory[] BuildNestedContextFactories(
            ContextControllerFactoryServiceContext serviceContext, ContextStateCache contextStateCache)
        {
            var nestedSpec = (ContextDetailNested) serviceContext.Detail;
            // determine nested filter use
            IDictionary<CreateContextDesc, IList<FilterSpecCompiled>> filtersPerNestedContext = null;
            for (int i = 0; i < nestedSpec.Contexts.Count; i++)
            {
                CreateContextDesc contextParent = nestedSpec.Contexts[i];
                for (int j = i + 1; j < nestedSpec.Contexts.Count; j++)
                {
                    CreateContextDesc contextControlled = nestedSpec.Contexts[j];
                    IList<FilterSpecCompiled> specs = contextControlled.FilterSpecs;
                    if (specs == null)
                    {
                        continue;
                    }
                    if (filtersPerNestedContext == null)
                    {
                        filtersPerNestedContext = new Dictionary<CreateContextDesc, IList<FilterSpecCompiled>>();
                    }
                    IList<FilterSpecCompiled> existing = filtersPerNestedContext.Get(contextParent);
                    if (existing != null)
                    {
                        existing.AddAll(specs);
                    }
                    else
                    {
                        filtersPerNestedContext.Put(contextParent, specs);
                    }
                }
            }

            // create contexts
            ICollection<String> namesUsed = new HashSet<String>();
            var hierarchy = new ContextControllerFactory[nestedSpec.Contexts.Count];
            for (int i = 0; i < nestedSpec.Contexts.Count; i++)
            {
                CreateContextDesc context = nestedSpec.Contexts[i];

                if (namesUsed.Contains(context.ContextName))
                {
                    throw new ExprValidationException(
                        "Context by name '" + context.ContextName +
                        "' has already been declared within nested context '" + serviceContext.ContextName + "'");
                }
                namesUsed.Add(context.ContextName);

                int nestingLevel = i + 1;

                IList<FilterSpecCompiled> optFiltersNested = null;
                if (filtersPerNestedContext != null)
                {
                    optFiltersNested = filtersPerNestedContext.Get(context);
                }

                hierarchy[i] = BuildContextFactory(
                    serviceContext, context.ContextName, context.ContextDetail, nestingLevel, optFiltersNested,
                    contextStateCache);
                hierarchy[i].ValidateFactory();
            }
            return hierarchy;
        }

        private static ContextControllerFactory BuildContextFactory(
            ContextControllerFactoryServiceContext serviceContext,
            String contextName,
            ContextDetail detail,
            int nestingLevel,
            IList<FilterSpecCompiled> optFiltersNested,
            ContextStateCache contextStateCache)
        {
            var factoryContext = new ContextControllerFactoryContext(
                serviceContext.ContextName, contextName, serviceContext.ServicesContext,
                serviceContext.AgentInstanceContextCreate, nestingLevel, serviceContext.IsRecoveringResilient);
            return BuildContextFactory(factoryContext, detail, optFiltersNested, contextStateCache);
        }

        private static ContextControllerFactory BuildContextFactory(ContextControllerFactoryContext factoryContext,
                                                                    ContextDetail detail,
                                                                    IList<FilterSpecCompiled> optFiltersNested,
                                                                    ContextStateCache contextStateCache)
        {
            return factoryContext.ServicesContext.ContextControllerFactoryFactorySvc.Make(factoryContext, detail, optFiltersNested, contextStateCache);
        }
    }
}
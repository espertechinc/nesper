///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.context.query;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.faf;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPFireAndForgetServiceImpl : EPFireAndForgetService
    {
        private readonly EPServicesContext services;
        private readonly AtomicBoolean serviceStatusProvider;

        public EPFireAndForgetServiceImpl(EPServicesContext services, AtomicBoolean serviceStatusProvider)
        {
            this.services = services;
            this.serviceStatusProvider = serviceStatusProvider;
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled)
        {
            return ExecuteQueryUnprepared(compiled, null);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled, ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null)
            {
                throw new ArgumentException("No context partition selectors provided");
            }
            return ExecuteQueryUnprepared(compiled, contextPartitionSelectors);
        }

        public EPFireAndForgetPreparedQuery PrepareQuery(EPCompiled compiled)
        {
            FAFProvider fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, services);
            FAFQueryMethodProvider queryMethodProvider = fafProvider.QueryMethodProvider;
            EPRuntimeHelperFAF.ValidateSubstitutionParams(queryMethodProvider);
            FAFQueryMethod queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready();
            return new EPPreparedQueryImpl(serviceStatusProvider, queryMethodProvider, queryMethod, services);
        }

        public EPFireAndForgetPreparedQueryParameterized PrepareQueryWithParameters(EPCompiled compiled)
        {
            FAFProvider fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, services);
            FAFQueryMethodProvider queryMethodProvider = fafProvider.QueryMethodProvider;
            FAFQueryMethod queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready();
            return new EPFireAndForgetPreparedQueryParameterizedImpl(serviceStatusProvider, queryMethodProvider.SubstitutionFieldSetter, queryMethod, queryMethodProvider.QueryInformationals);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPFireAndForgetPreparedQueryParameterized parameterizedQuery)
        {
            return ExecuteQueryPrepared(parameterizedQuery, null);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPFireAndForgetPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] selectors)
        {
            return ExecuteQueryPrepared(parameterizedQuery, selectors);
        }

        private EPFireAndForgetQueryResult ExecuteQueryPrepared(EPFireAndForgetPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] selectors)
        {
            EPFireAndForgetPreparedQueryParameterizedImpl impl = (EPFireAndForgetPreparedQueryParameterizedImpl) parameterizedQuery;
            EPRuntimeHelperFAF.CheckSubstitutionSatisfied(impl);
            if (!impl.ServiceProviderStatus.Get())
            {
                throw FAFQueryMethodUtil.RuntimeDestroyed();
            }
            if (impl.ServiceProviderStatus != serviceStatusProvider)
            {
                throw new EPException("Service provider has already been destroyed and reallocated");
            }
            return new EPQueryResultImpl(impl.QueryMethod.Execute(serviceStatusProvider, impl.Fields, selectors, services.ContextManagementService));
        }

        private EPFireAndForgetQueryResult ExecuteQueryUnprepared(EPCompiled compiled, ContextPartitionSelector[] contextPartitionSelectors)
        {
            FAFProvider fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, services);
            FAFQueryMethodProvider queryMethodProvider = fafProvider.QueryMethodProvider;
            EPRuntimeHelperFAF.ValidateSubstitutionParams(queryMethodProvider);
            FAFQueryMethod queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready();
            EPPreparedQueryResult result = queryMethod.Execute(serviceStatusProvider, queryMethodProvider.SubstitutionFieldSetter, contextPartitionSelectors, services.ContextManagementService);
            return new EPQueryResultImpl(result);
        }
    }
} // end of namespace
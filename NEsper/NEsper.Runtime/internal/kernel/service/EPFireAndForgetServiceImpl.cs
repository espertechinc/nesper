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
    public class EpFireAndForgetServiceImpl : EPFireAndForgetService
    {
        private readonly EPServicesContext _services;
        private readonly AtomicBoolean _serviceStatusProvider;

        public EpFireAndForgetServiceImpl(
            EPServicesContext services,
            AtomicBoolean serviceStatusProvider)
        {
            _services = services;
            _serviceStatusProvider = serviceStatusProvider;
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled)
        {
            return ExecuteQueryUnprepared(compiled, null);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(
            EPCompiled compiled,
            ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null) {
                throw new ArgumentException("No context partition selectors provided");
            }

            return ExecuteQueryUnprepared(compiled, contextPartitionSelectors);
        }

        public EPFireAndForgetPreparedQuery PrepareQuery(EPCompiled compiled)
        {
            var fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, _services);
            var queryMethodProvider = fafProvider.QueryMethodProvider;
            EPRuntimeHelperFAF.ValidateSubstitutionParams(queryMethodProvider);
            var queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready(_services.StatementContextRuntimeServices);
            return new EPPreparedQueryImpl(_serviceStatusProvider, queryMethodProvider, queryMethod, _services);
        }

        public EPFireAndForgetPreparedQueryParameterized PrepareQueryWithParameters(EPCompiled compiled)
        {
            var fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, _services);
            var queryMethodProvider = fafProvider.QueryMethodProvider;
            var queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready(_services.StatementContextRuntimeServices);
            return new EPFireAndForgetPreparedQueryParameterizedImpl(
                _serviceStatusProvider,
                queryMethodProvider.SubstitutionFieldSetter,
                queryMethod,
                queryMethodProvider.QueryInformationals);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(EPFireAndForgetPreparedQueryParameterized parameterizedQuery)
        {
            return ExecuteQueryPrepared(parameterizedQuery, null);
        }

        public EPFireAndForgetQueryResult ExecuteQuery(
            EPFireAndForgetPreparedQueryParameterized parameterizedQuery,
            ContextPartitionSelector[] selectors)
        {
            return ExecuteQueryPrepared(parameterizedQuery, selectors);
        }

        private EPFireAndForgetQueryResult ExecuteQueryPrepared(
            EPFireAndForgetPreparedQueryParameterized parameterizedQuery,
            ContextPartitionSelector[] selectors)
        {
            var impl = (EPFireAndForgetPreparedQueryParameterizedImpl) parameterizedQuery;
            EPRuntimeHelperFAF.CheckSubstitutionSatisfied(impl);
            if (!impl.ServiceProviderStatus.Get()) {
                throw FAFQueryMethodUtil.RuntimeDestroyed();
            }

            if (impl.ServiceProviderStatus != _serviceStatusProvider) {
                throw new EPException("Service provider has already been destroyed and reallocated");
            }

            return new EPQueryResultImpl(impl.QueryMethod.Execute(_serviceStatusProvider, impl.Fields, selectors, _services.ContextManagementService));
        }

        private EPFireAndForgetQueryResult ExecuteQueryUnprepared(
            EPCompiled compiled,
            ContextPartitionSelector[] contextPartitionSelectors)
        {
            var fafProvider = EPRuntimeHelperFAF.QueryMethod(compiled, _services);
            var queryMethodProvider = fafProvider.QueryMethodProvider;
            EPRuntimeHelperFAF.ValidateSubstitutionParams(queryMethodProvider);
            var queryMethod = queryMethodProvider.QueryMethod;
            queryMethod.Ready(_services.StatementContextRuntimeServices);
            var result = queryMethod.Execute(
                _serviceStatusProvider,
                queryMethodProvider.SubstitutionFieldSetter,
                contextPartitionSelectors,
                _services.ContextManagementService);
            return new EPQueryResultImpl(result);
        }
    }
} // end of namespace
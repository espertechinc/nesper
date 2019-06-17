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
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.faf
{
    /// <summary>
    ///     Provides prepared query functionality.
    /// </summary>
    public class EPPreparedQueryImpl : EPFireAndForgetPreparedQuery
    {
        private readonly EPServicesContext epServicesContext;
        private readonly FAFQueryMethod queryMethod;
        private readonly FAFQueryMethodProvider queryMethodProvider;
        private readonly AtomicBoolean serviceStatusProvider;

        public EPPreparedQueryImpl(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodProvider queryMethodProvider,
            FAFQueryMethod queryMethod,
            EPServicesContext epServicesContext)
        {
            this.serviceStatusProvider = serviceStatusProvider;
            this.queryMethodProvider = queryMethodProvider;
            this.queryMethod = queryMethod;
            this.epServicesContext = epServicesContext;
        }

        public EPFireAndForgetQueryResult Execute()
        {
            return ExecuteInternal(null);
        }

        public EPFireAndForgetQueryResult Execute(ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null) {
                throw new ArgumentException("No context partition selectors provided");
            }

            return ExecuteInternal(contextPartitionSelectors);
        }

        public EventType EventType => queryMethodProvider.QueryMethod.EventType;

        private EPFireAndForgetQueryResult ExecuteInternal(ContextPartitionSelector[] contextPartitionSelectors)
        {
            try {
                FAFQueryMethodAssignerSetter setter = queryMethodProvider.SubstitutionFieldSetter;
                var result = queryMethod.Execute(
                    serviceStatusProvider, setter, contextPartitionSelectors, epServicesContext.ContextManagementService);
                return new EPQueryResultImpl(result);
            }
            catch (Exception ex) {
                throw new EPException(ex.Message, ex);
            }
        }
    }
} // end of namespace
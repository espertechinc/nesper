///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectSessionUnprepared : FAFQuerySessionUnprepared
    {
        private readonly FAFQueryMethodSelect select;

        public FAFQueryMethodSelectSessionUnprepared(FAFQueryMethodSelect select)
        {
            this.select = select;
        }

        public EPPreparedQueryResult Execute(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            return ExecuteSelect(
                select,
                serviceStatusProvider,
                assignerSetter,
                contextPartitionSelectors,
                contextManagementService);
        }

        internal static EPPreparedQueryResult ExecuteSelect(
            FAFQueryMethodSelect select,
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            if (!serviceStatusProvider.Get()) {
                throw FAFQueryMethodUtil.RuntimeDestroyed();
            }

            var processors = select.Processors;
            if (processors.Length > 0 &&
                contextPartitionSelectors != null &&
                contextPartitionSelectors.Length != processors.Length) {
                throw new ArgumentException(
                    "The number of context partition selectors does not match the number of named windows or tables in the from-clause");
            }

            try {
                return select.SelectExec.Execute(
                    select,
                    contextPartitionSelectors,
                    assignerSetter,
                    contextManagementService);
            }
            finally {
                if (select.HasTableAccess) {
                    select.SelectExec.ReleaseTableLocks(processors);
                }
            }
        }
    }
} // end of namespace
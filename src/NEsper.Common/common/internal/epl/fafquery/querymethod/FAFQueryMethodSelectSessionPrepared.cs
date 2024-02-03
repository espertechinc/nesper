///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectSessionUnprepared;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelectSessionPrepared : FAFQueryMethodSessionPrepared
    {
        private readonly FAFQueryMethodSelect select;

        public FAFQueryMethodSelectSessionPrepared(FAFQueryMethodSelect select)
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

        public void Close()
        {
            select.SelectExec.Close();
        }
    }
} // end of namespace
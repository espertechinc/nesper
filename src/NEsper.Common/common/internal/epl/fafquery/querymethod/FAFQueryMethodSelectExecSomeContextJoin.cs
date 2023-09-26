///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecSomeContextJoin : FAFQueryMethodSelectExec
    {
        public static readonly FAFQueryMethodSelectExec INSTANCE = new FAFQueryMethodSelectExecSomeContextJoin();

        private FAFQueryMethodSelectExecSomeContextJoin()
        {
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            throw NotImplemented();
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            throw NotImplemented();
        }

        private UnsupportedOperationException NotImplemented()
        {
            return new UnsupportedOperationException("Context with join is not supported");
        }
    }
} // end of namespace
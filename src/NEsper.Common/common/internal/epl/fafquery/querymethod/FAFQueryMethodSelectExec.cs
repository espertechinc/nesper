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

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public interface FAFQueryMethodSelectExec
    {
        EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService);

        void ReleaseTableLocks(FireAndForgetProcessor[] processors);

        // default void prepare(FAFQueryMethodSelect fafQueryMethodSelect) {}
        void Prepare(FAFQueryMethodSelect fafQueryMethodSelect)
        {
        }

        // default void close() {};
        void Close()
        {
        }
    }
} // end of namespace
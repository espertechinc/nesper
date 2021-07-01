///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.controller.condition;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermSvcEntry
    {
        public ContextControllerInitTermSvcEntry(
            int subpathIdOrCPId,
            ContextControllerConditionNonHA terminationCondition,
            ContextControllerInitTermPartitionKey partitionKey)
        {
            SubpathIdOrCPId = subpathIdOrCPId;
            TerminationCondition = terminationCondition;
            PartitionKey = partitionKey;
        }

        public int SubpathIdOrCPId { get; }

        public ContextControllerConditionNonHA TerminationCondition { get; }

        public ContextControllerInitTermPartitionKey PartitionKey { get; }
    }
} // end of namespace
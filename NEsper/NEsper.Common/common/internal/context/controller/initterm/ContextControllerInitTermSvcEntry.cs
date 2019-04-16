///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermSvcEntry
    {
        private readonly int subpathIdOrCPId;
        private readonly ContextControllerConditionNonHA terminationCondition;
        private readonly ContextControllerInitTermPartitionKey partitionKey;

        public ContextControllerInitTermSvcEntry(
            int subpathIdOrCPId,
            ContextControllerConditionNonHA terminationCondition,
            ContextControllerInitTermPartitionKey partitionKey)
        {
            this.subpathIdOrCPId = subpathIdOrCPId;
            this.terminationCondition = terminationCondition;
            this.partitionKey = partitionKey;
        }

        public int SubpathIdOrCPId {
            get => subpathIdOrCPId;
        }

        public ContextControllerConditionNonHA TerminationCondition {
            get => terminationCondition;
        }

        public ContextControllerInitTermPartitionKey PartitionKey {
            get => partitionKey;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.controller.condition;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedSvcEntry
    {
        public ContextControllerKeyedSvcEntry(
            int subpathOrCPId,
            ContextControllerConditionNonHA terminationCondition)
        {
            SubpathOrCPId = subpathOrCPId;
            TerminationCondition = terminationCondition;
        }

        public int SubpathOrCPId { get; }

        public ContextControllerConditionNonHA TerminationCondition { get; }
    }
} // end of namespace
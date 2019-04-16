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

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedSvcEntry
    {
        private readonly int subpathOrCPId;
        private readonly ContextControllerConditionNonHA terminationCondition;

        public ContextControllerKeyedSvcEntry(
            int subpathOrCPId,
            ContextControllerConditionNonHA terminationCondition)
        {
            this.subpathOrCPId = subpathOrCPId;
            this.terminationCondition = terminationCondition;
        }

        public int SubpathOrCPId {
            get => subpathOrCPId;
        }

        public ContextControllerConditionNonHA TerminationCondition {
            get => terminationCondition;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedFilterEntryWInit : ContextControllerKeyedFilterEntry
    {
        private readonly ContextConditionDescriptorFilter _initCond;

        public ContextControllerKeyedFilterEntryWInit(
            ContextControllerKeyedImpl callback,
            IntSeqKey controllerPath,
            ContextControllerDetailKeyedItem item,
            object[] parentPartitionKeys,
            ContextConditionDescriptorFilter initCond)
            : base(callback, controllerPath, item, parentPartitionKeys)
        {
            this._initCond = initCond;
            Start(initCond.FilterSpecActivatable);
        }

        public override void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            callback.MatchFound(item, theEvent, controllerPath, _initCond.OptionalFilterAsName);
        }

        public override void Destroy()
        {
            Stop(_initCond.FilterSpecActivatable);
        }
    }
} // end of namespace
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
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedFilterEntryNoInit : ContextControllerKeyedFilterEntry
    {
        public ContextControllerKeyedFilterEntryNoInit(
            ContextControllerKeyedImpl callback,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys,
            ContextControllerDetailKeyedItem item)
            : base(callback, controllerPath, item, parentPartitionKeys)
        {
            Start(item.FilterSpecActivatable);
        }

        public override void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            callback.MatchFound(item, theEvent, controllerPath, item.AliasName);
        }

        public override void Destroy()
        {
            Stop(item.FilterSpecActivatable);
        }
    }
} // end of namespace
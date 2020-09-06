///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeActionDel : InfraOnMergeAction
    {
        public InfraOnMergeActionDel(ExprEvaluator optionalFilter)
            : base(optionalFilter)

        {
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext)
        {
            oldData.Add(matchingEvent);
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance tableStateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext)
        {
            tableStateInstance.DeleteEvent(matchingEvent);
            changeHandlerRemoved?.Add(matchingEvent, eventsPerStream, false, agentInstanceContext);
        }

        public override string Name {
            get { return "delete"; }
        }
    }
} // end of namespace
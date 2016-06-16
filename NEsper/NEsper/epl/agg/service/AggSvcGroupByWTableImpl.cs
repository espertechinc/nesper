///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByWTableImpl : AggSvcGroupByWTableBase
    {
        public AggSvcGroupByWTableImpl(
            TableMetadata tableMetadata,
            TableColumnMethodPair[] methodPairs,
            AggregationAccessorSlotPair[] accessors,
            bool join,
            TableStateInstanceGrouped tableStateInstance,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents)
            : base(tableMetadata, methodPairs, accessors, join, tableStateInstance, targetStates, accessStateExpr, agents)
        {
        }

        public override void ApplyEnterInternal(
            EventBean[] eventsPerStream,
            Object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ApplyEnterGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
        }

        public override void ApplyLeaveInternal(
            EventBean[] eventsPerStream,
            Object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ApplyLeaveGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
        }
    }
}

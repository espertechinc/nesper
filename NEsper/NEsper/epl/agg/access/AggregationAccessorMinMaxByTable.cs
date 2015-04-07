///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorMinMaxByTable : AggregationAccessorMinMaxByBase
    {
        private readonly TableMetadata tableMetadata;
    
        public AggregationAccessorMinMaxByTable(bool max, TableMetadata tableMetadata)
            : base(max)
        {
            this.tableMetadata = tableMetadata;
        }
    
        public override object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            EventBean @event = GetEnumerableEvent(state, eventsPerStream, isNewData, context);
            if (@event == null) {
                return null;
            }
            return tableMetadata.EventToPublic.ConvertToUnd(@event, eventsPerStream, isNewData, context);
        }
    }
}

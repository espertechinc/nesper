///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorMinMaxByTable : AggregationAccessorMinMaxByBase
    {
        private readonly TableMetadata _tableMetadata;
    
        public AggregationAccessorMinMaxByTable(bool max, TableMetadata tableMetadata)
            : base(max)
        {
            _tableMetadata = tableMetadata;
        }
    
        public override object GetValue(AggregationState state, EvaluateParams evaluateParams)
        {
            EventBean @event = GetEnumerableEvent(state, evaluateParams);
            if (@event == null) {
                return null;
            }
            return _tableMetadata.EventToPublic.ConvertToUnd(@event, evaluateParams);
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public abstract class TableInstanceGroupedBase : TableInstanceBase, TableInstanceGrouped
    {
        protected TableInstanceGroupedBase(
            Table table,
            AgentInstanceContext agentInstanceContext)
            : base(table, agentInstanceContext)
        {
        }

        public override void AddEvent(EventBean @event)
        {
            agentInstanceContext.InstrumentationProvider.QTableAddEvent(@event);

            try {
                foreach (EventTable table in indexRepository.Tables) {
                    table.Add(@event, agentInstanceContext);
                }
            }
            catch (EPException) {
                foreach (EventTable table in indexRepository.Tables) {
                    table.Remove(@event, agentInstanceContext);
                }

                throw;
            }
            finally {
                agentInstanceContext.InstrumentationProvider.ATableAddEvent();
            }
        }

        protected ObjectArrayBackedEventBean CreateRowIntoTable(object groupKeys)
        {
            EventType eventType = table.MetaData.InternalEventType;
            AggregationRow aggregationRow = table.AggregationRowFactory.Make();
            var data = new object[eventType.PropertyDescriptors.Count];
            data[0] = aggregationRow;

            int[] groupKeyColNums = table.MetaData.KeyColNums;
            if (groupKeyColNums.Length == 1) {
                if (groupKeys is MultiKeyArrayWrap multiKeyArrayWrap) {
                    data[groupKeyColNums[0]] = multiKeyArrayWrap.Array;
                } else {
                    data[groupKeyColNums[0]] = groupKeys;
                }
            }
            else {
                var mk = (MultiKey) groupKeys;
                for (var i = 0; i < groupKeyColNums.Length; i++) {
                    data[groupKeyColNums[i]] = mk.GetKey(i);
                }
            }

            ObjectArrayBackedEventBean row =
                agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedObjectArray(data, eventType);
            AddEvent(row);
            return row;
        }

        public abstract ObjectArrayBackedEventBean GetRowForGroupKey(object groupKey);

        public abstract ObjectArrayBackedEventBean GetCreateRowIntoTable(
            object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract ICollection<object> GroupKeysMayMultiKey { get; }
        public abstract ICollection<object> GroupKeys { get; }
    }
} // end of namespace
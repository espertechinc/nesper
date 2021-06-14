///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public abstract class TableInstanceUngroupedBase : TableInstanceBase,
        TableInstanceUngrouped
    {
        public TableInstanceUngroupedBase(
            Table table,
            AgentInstanceContext agentInstanceContext)
            : base(
                table,
                agentInstanceContext)
        {
        }

        public override void AddExplicitIndex(
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            bool isRecoveringResilient)
        {
            throw new UnsupportedOperationException("Ungrouped tables do not allow explicit indexes");
        }

        public override void RemoveExplicitIndex(string indexName)
        {
            throw new UnsupportedOperationException("Ungrouped tables do not allow explicit indexes");
        }

        public abstract ObjectArrayBackedEventBean EventUngrouped { get; }
        public abstract ObjectArrayBackedEventBean GetCreateRowIntoTable(ExprEvaluatorContext exprEvaluatorContext);

        protected ObjectArrayBackedEventBean CreateRowIntoTable()
        {
            var eventType = table.MetaData.InternalEventType;
            var aggregationRow = table.AggregationRowFactory.Make();
            var data = new object[eventType.PropertyDescriptors.Count];
            data[0] = aggregationRow;
            var row = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedObjectArray(data, eventType);
            AddEvent(row);
            return row;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableStateViewableInternal : ViewSupport
    {
        private readonly ExprEvaluator optionalTableFilter;

        private readonly TableInstance tableInstance;

        public TableStateViewableInternal(
            TableInstance tableInstance,
            ExprEvaluator optionalTableFilter)
        {
            this.tableInstance = tableInstance;
            this.optionalTableFilter = optionalTableFilter;
        }

        public override EventType EventType => tableInstance.Table.MetaData.InternalEventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // no action required
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            using (var enumerator = tableInstance.EventCollection.GetEnumerator()) {
                if (optionalTableFilter != null) {
                    return FilteredEventEnumerator.For(
                        optionalTableFilter,
                        enumerator,
                        tableInstance.AgentInstanceContext);
                }
            }

            return tableInstance.EventCollection.GetEnumerator();
        }
    }
} // end of namespace
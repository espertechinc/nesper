///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableStateViewableInternal : ViewSupport
    {
        private readonly TableMetadata _tableMetadata;
        private readonly TableStateInstance _tableStateInstance;
        private readonly ExprEvaluator[] _optionalTableFilters;
    
        public TableStateViewableInternal(TableMetadata tableMetadata, TableStateInstance tableStateInstance, ExprEvaluator[] optionalTableFilters)
        {
            _tableMetadata = tableMetadata;
            _tableStateInstance = tableStateInstance;
            _optionalTableFilters = optionalTableFilters;
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // no action required
        }

        public override EventType EventType
        {
            get { return _tableMetadata.InternalEventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator() {
            if (_optionalTableFilters != null)
            {
                return FilteredEventEnumerator.Enumerate(
                    _optionalTableFilters,
                    _tableStateInstance.EventCollection,
                    _tableStateInstance.AgentInstanceContext);
            }
            return _tableStateInstance.EventCollection.GetEnumerator();
        }
    }
}

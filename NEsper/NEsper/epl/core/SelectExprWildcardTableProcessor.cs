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

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor for select-clause expressions that handles wildcards for single streams with no insert-into.
    /// </summary>
    public class SelectExprWildcardTableProcessor : SelectExprProcessor
    {
        private readonly TableMetadata metadata;
    
        public SelectExprWildcardTableProcessor(string tableName, TableService tableService) {
            metadata = tableService.GetTableMetadata(tableName);
        }
    
        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean @event = eventsPerStream[0];
            if (@event == null) {
                return null;
            }
            return metadata.GetPublicEventBean(@event, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public EventType ResultEventType
        {
            get { return metadata.PublicEventType; }
        }
    }
}

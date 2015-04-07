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
    /// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectExprJoinWildcardProcessorTableRows : SelectExprProcessor
    {
        private readonly SelectExprProcessor inner;
        private readonly EventBean[] eventsPerStreamWTableRows;
        private readonly TableMetadata[] tables;
    
        public SelectExprJoinWildcardProcessorTableRows(EventType[] types, SelectExprProcessor inner, TableService tableService) {
            this.inner = inner;
            eventsPerStreamWTableRows = new EventBean[types.Length];
            tables = new TableMetadata[types.Length];
            for (int i = 0; i < types.Length; i++) {
                tables[i] = tableService.GetTableMetadataFromEventType(types[i]);
            }
        }
    
        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            for (int i = 0; i < eventsPerStreamWTableRows.Length; i++) {
                if (tables[i] != null && eventsPerStream[i] != null) {
                    eventsPerStreamWTableRows[i] = tables[i].EventToPublic.Convert(eventsPerStream[i], eventsPerStream, isNewData, exprEvaluatorContext);
                }
                else {
                    eventsPerStreamWTableRows[i] = eventsPerStream[i];
                }
            }
            return inner.Process(eventsPerStreamWTableRows, isNewData, isSynthesize, exprEvaluatorContext);
        }

        public EventType ResultEventType
        {
            get { return inner.ResultEventType; }
        }
    }
}

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

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectExprJoinWildcardProcessorTableRows : SelectExprProcessor
    {
        private readonly SelectExprProcessor _inner;
        private readonly EventBean[] _eventsPerStreamWTableRows;
        private readonly TableMetadata[] _tables;
    
        public SelectExprJoinWildcardProcessorTableRows(EventType[] types, SelectExprProcessor inner, TableService tableService)
        {
            _inner = inner;
            _eventsPerStreamWTableRows = new EventBean[types.Length];
            _tables = new TableMetadata[types.Length];
            for (int i = 0; i < types.Length; i++) {
                _tables[i] = tableService.GetTableMetadataFromEventType(types[i]);
            }
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            for (int i = 0; i < _eventsPerStreamWTableRows.Length; i++)
            {
                if (_tables[i] != null && eventsPerStream[i] != null)
                {
                    _eventsPerStreamWTableRows[i] = _tables[i].EventToPublic.Convert(eventsPerStream[i], evaluateParams);
                }
                else
                {
                    _eventsPerStreamWTableRows[i] = eventsPerStream[i];
                }
            }
            return _inner.Process(_eventsPerStreamWTableRows, isNewData, isSynthesize, exprEvaluatorContext);
        }

        public EventType ResultEventType
        {
            get { return _inner.ResultEventType; }
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.merge
{
    public class TableOnMergeActionIns : TableOnMergeAction
    {
        private readonly bool _audit;
        private readonly SelectExprProcessor _insertHelper;
        private readonly InternalEventRouteDest _internalEventRouteDest;
        private readonly InternalEventRouter _internalEventRouter;
        private readonly EPStatementHandle _statementHandle;
        private readonly TableStateRowFactory _tableStateRowFactory;

        public TableOnMergeActionIns(
            ExprEvaluator optionalFilter,
            SelectExprProcessor insertHelper,
            InternalEventRouter internalEventRouter,
            EPStatementHandle statementHandle,
            InternalEventRouteDest internalEventRouteDest,
            bool audit,
            TableStateRowFactory tableStateRowFactory)
            : base(optionalFilter)
        {
            _insertHelper = insertHelper;
            _internalEventRouter = internalEventRouter;
            _statementHandle = statementHandle;
            _internalEventRouteDest = internalEventRouteDest;
            _audit = audit;
            _tableStateRowFactory = tableStateRowFactory;
        }

        public override string Name
        {
            get { return _internalEventRouter != null ? "insert-into" : "select"; }
        }

        public bool IsInsertIntoBinding
        {
            get { return _internalEventRouter == null; }
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableStateInstance tableStateInstance,
            TableOnMergeViewChangeHandler changeHandlerAdded,
            TableOnMergeViewChangeHandler changeHandlerRemoved,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = _insertHelper.Process(eventsPerStream, true, true, exprEvaluatorContext);
            if (_internalEventRouter == null)
            {
                var aggs = _tableStateRowFactory.MakeAggs(exprEvaluatorContext.AgentInstanceId, null, null, tableStateInstance.AggregationServicePassThru);
                ((object[]) theEvent.Underlying)[0] = aggs;
                tableStateInstance.AddEvent(theEvent);
                if (changeHandlerAdded != null)
                {
                    changeHandlerAdded.Add(theEvent, eventsPerStream, true, exprEvaluatorContext);
                }
                return;
            }

            if (_audit)
            {
                AuditPath.AuditInsertInto(_internalEventRouteDest.EngineURI, _statementHandle.StatementName, theEvent);
            }
            _internalEventRouter.Route(theEvent, _statementHandle, _internalEventRouteDest, exprEvaluatorContext, false);
        }
    }
}
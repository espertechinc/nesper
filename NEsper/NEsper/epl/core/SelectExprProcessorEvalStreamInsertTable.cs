///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    public class SelectExprProcessorEvalStreamInsertTable : ExprEvaluator
    {
        private readonly int _streamNum;
        private readonly ExprStreamUnderlyingNode _undNode;
        private readonly TableMetadata _tableMetadata;
        private readonly Type _returnType;
    
        public SelectExprProcessorEvalStreamInsertTable(int streamNum, ExprStreamUnderlyingNode undNode, TableMetadata tableMetadata, Type returnType)
        {
            _streamNum = streamNum;
            _undNode = undNode;
            _tableMetadata = tableMetadata;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return this.Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QExprStreamUndSelectClause(_undNode);
            }

            var @event = eventsPerStream == null ? null : eventsPerStream[_streamNum];
            if (@event != null) {
                @event = _tableMetadata.EventToPublic.Convert(@event, eventsPerStream, isNewData, exprEvaluatorContext);
            }
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AExprStreamUndSelectClause(@event);
            }
            return @event;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
} // end of namespace

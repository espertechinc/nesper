///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprNodeUtilExprEvalStreamNumEventTable : ExprEvaluator
    {
        private readonly int _streamNum;
        private readonly TableMetadata _tableMetadata;
    
        public ExprNodeUtilExprEvalStreamNumEventTable(int streamNum, TableMetadata tableMetadata)
        {
            _streamNum = streamNum;
            _tableMetadata = tableMetadata;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_streamNum];
            if (@event == null) {
                return null;
            }
            return _tableMetadata.EventToPublic.Convert(@event, eventsPerStream, isNewData, context);
        }

        public Type ReturnType
        {
            get { return typeof (EventBean); }
        }
    }
}

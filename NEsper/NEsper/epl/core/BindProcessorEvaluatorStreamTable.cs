///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.core
{
    public class BindProcessorEvaluatorStreamTable : ExprEvaluator
    {
        private readonly int _streamNum;
        private readonly Type _returnType;
        private readonly TableMetadata _tableMetadata;
    
        public BindProcessorEvaluatorStreamTable(int streamNum, Type returnType, TableMetadata tableMetadata)
        {
            _streamNum = streamNum;
            _returnType = returnType;
            _tableMetadata = tableMetadata;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            EventBean theEvent = eventsPerStream[_streamNum];
            if (theEvent != null) {
                return _tableMetadata.EventToPublic.ConvertToUnd(theEvent, eventsPerStream, isNewData, exprEvaluatorContext);
            }
            return null;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}

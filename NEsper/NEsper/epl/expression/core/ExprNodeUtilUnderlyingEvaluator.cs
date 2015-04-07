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

using Microsoft.JScript;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprNodeUtilUnderlyingEvaluator : ExprEvaluator {
        private readonly int streamNum;
        private readonly Type resultType;
    
        public ExprNodeUtilUnderlyingEvaluator(int streamNum, Type resultType) {
            this.streamNum = streamNum;
            this.resultType = resultType;
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
            if ((eventsPerStream == null) || (eventsPerStream[streamNum] == null)) {
                return null;
            }
            return eventsPerStream[streamNum].Underlying;
        }

        public Type ReturnType
        {
            get { return resultType; }
        }
    }
}

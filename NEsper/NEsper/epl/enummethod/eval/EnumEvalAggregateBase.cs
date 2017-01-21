///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAggregateBase
    {
        protected ExprEvaluator Initialization;
        protected ExprEvaluator InnerExpression;
        protected int StreamNumLambda;
        protected ObjectArrayEventType ResultEventType;
    
        public EnumEvalAggregateBase(ExprEvaluator initialization,
                                     ExprEvaluator innerExpression, int streamNumLambda,
                                     ObjectArrayEventType resultEventType)
        {
            Initialization = initialization;
            InnerExpression = innerExpression;
            StreamNumLambda = streamNumLambda;
            ResultEventType = resultEventType;
        }

        public int StreamNumSize
        {
            get { return StreamNumLambda + 2; }
        }
    }
}

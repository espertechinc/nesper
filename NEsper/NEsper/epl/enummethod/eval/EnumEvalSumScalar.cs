///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalSumScalar 
        : EnumEvalBase
        , EnumEval
    {
        private readonly ExprDotEvalSumMethodFactory _sumMethodFactory;
    
        public EnumEvalSumScalar(int streamCountIncoming, ExprDotEvalSumMethodFactory sumMethodFactory)
            : base(streamCountIncoming)
        {
            _sumMethodFactory = sumMethodFactory;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var method = _sumMethodFactory.SumAggregator;
            foreach (Object next in target) {
                method.Enter(next);
            }
            return method.Value;
        }
    }
}

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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalToMapEvents
        : EnumEvalBase
        , EnumEval
    {
        private readonly ExprEvaluator _secondExpression;

        public EnumEvalToMapEvents(ExprEvaluator innerExpression, int streamCountIncoming, ExprEvaluator secondExpression)
            : base(innerExpression, streamCountIncoming)
        {
            _secondExpression = secondExpression;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var map = new Dictionary<object, object>().WithNullSupport();

            foreach (EventBean next in target)
            {
                eventsLambda[StreamNumLambda] = next;

                var key = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                var value = _secondExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                map.Put(key, value);
            }

            return map;
        }
    }
}

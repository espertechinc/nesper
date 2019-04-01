///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class EnumEvalGroupByKeySelectorEvents 
        : EnumEvalBase
        , EnumEval
    {
        public EnumEvalGroupByKeySelectorEvents(ExprEvaluator innerExpression, int streamCountIncoming)
                    : base(innerExpression, streamCountIncoming)
        {
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var result = new LinkedHashMap<Object, ICollection<object>>();
            foreach (EventBean next in target)
            {
                eventsLambda[StreamNumLambda] = next;

                var key = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                var value = result.Get(key);
                if (value == null)
                {
                    value = new List<object>();
                    result.Put(key, value);
                }
                value.Add(next.Underlying);
            }

            return result;
        }
    }
}

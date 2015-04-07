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
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalLastOfPredicateScalar
        : EnumEvalBaseScalar
        , EnumEval
    {
        public EnumEvalLastOfPredicateScalar(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType type)
            : base(innerExpression, streamCountIncoming, type)
        {
        }

        public override object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            Object result = null;
            ObjectArrayEventBean evalEvent = new ObjectArrayEventBean(new Object[1], Type);

            foreach (var next in target)
            {
                evalEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = evalEvent;

                Object pass = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.AsBoolean())
                {
                    continue;
                }

                result = next;
            }

            return result;
        }
    }
}

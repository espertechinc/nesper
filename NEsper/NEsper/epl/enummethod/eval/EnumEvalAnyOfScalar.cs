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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAnyOfScalar
        : EnumEvalBaseScalar
        , EnumEval
    {
        public EnumEvalAnyOfScalar(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType type)
            : base(innerExpression, streamCountIncoming, type)
        {
        }

        public override Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty())
            {
                return false;
            }

            var evalEvent = new ObjectArrayEventBean(new Object[1], Type);
            foreach (Object next in target)
            {

                evalEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = evalEvent;

                var pass = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context)).AsBoolean();
                if (pass)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

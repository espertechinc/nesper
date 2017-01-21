///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public abstract class EnumEvalBaseScalar
        : EnumEvalBase
        , EnumEval
    {
        protected readonly ObjectArrayEventType Type;

        protected EnumEvalBaseScalar(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType type)
            : base(innerExpression, streamCountIncoming)
        {
            Type = type;
        }

        public abstract object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context);
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public abstract class EnumEvalBaseIndex : EnumEval
    {
        protected ExprEvaluator InnerExpression;
        protected int StreamNumLambda;
        protected ObjectArrayEventType IndexEventType;

        protected EnumEvalBaseIndex(ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType)
        {
            InnerExpression = innerExpression;
            StreamNumLambda = streamNumLambda;
            IndexEventType = indexEventType;
        }

        public int StreamNumSize
        {
            get { return StreamNumLambda + 2; }
        }

        public abstract object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context);
    }
}

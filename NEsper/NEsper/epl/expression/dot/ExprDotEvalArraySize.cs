///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEvalArraySize : ExprDotEval
    {
        public Object Evaluate(Object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var asArray = target as Array;
            if (asArray == null) {
                return null;
            }
            return asArray.Length;
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.SingleValue(typeof(int)); }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitArrayLength();
        }
    }
}

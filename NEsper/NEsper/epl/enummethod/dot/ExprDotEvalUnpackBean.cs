///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalUnpackBean : ExprDotEval
    {
        private readonly EPType _returnType;
    
        public ExprDotEvalUnpackBean(EventType lambdaType)
        {
            _returnType = EPTypeHelper.SingleValue(lambdaType.UnderlyingType);
        }

        public object Evaluate(object target, EvaluateParams evalParams)
        {
            var theEvent = target as EventBean;
            return theEvent != null ? theEvent.Underlying : null;
        }

        public EPType TypeInfo
        {
            get { return _returnType; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEvent();
        }
    }
}

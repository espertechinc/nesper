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
    public class ExprDotEvalProperty : ExprDotEval
    {
        private readonly EventPropertyGetter _getter;
        private readonly EPType _returnType;

        public ExprDotEvalProperty(EventPropertyGetter getter, EPType returnType)
        {
            _getter = getter;
            _returnType = returnType;
        }

        public object Evaluate(object target, EvaluateParams evalParams)
        {
            return target is EventBean ? _getter.Get((EventBean) target) : null;
        }

        public EPType TypeInfo
        {
            get { return _returnType; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitPropertySource();
        }
    }
}

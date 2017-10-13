///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.declexpr
{
    public class ExprDeclaredEvalConstant : ExprEvaluator
    {
        private readonly Type _returnType;
        private readonly ExpressionDeclItem _prototype;
        private readonly Object _value;
    
        public ExprDeclaredEvalConstant(Type returnType, ExpressionDeclItem prototype, Object value)
        {
            _returnType = returnType;
            _prototype = prototype;
            _value = value;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDeclared(_prototype);}
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDeclared(_value);}
            return _value;
        }
    }
}

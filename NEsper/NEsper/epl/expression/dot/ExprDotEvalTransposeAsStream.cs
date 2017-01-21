///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.dot
{
    [Serializable]
    public class ExprDotEvalTransposeAsStream : ExprEvaluator
    {
        private readonly ExprEvaluator _inner;

        public ExprDotEvalTransposeAsStream(ExprEvaluator inner)
        {
            _inner = inner;
        }

        public Type ReturnType
        {
            get { return _inner.ReturnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return _inner.Evaluate(evaluateParams);
        }
    }
}

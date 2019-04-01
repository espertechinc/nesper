///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalAvroArrayCoercer : ExprEvaluator {
        private readonly ExprEvaluator _eval;
        private readonly TypeWidener _widener;
    
        public SelectExprProcessorEvalAvroArrayCoercer(ExprEvaluator eval, TypeWidener widener) {
            _eval = eval;
            _widener = widener;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var result = _eval.Evaluate(evaluateParams);
            return _widener.Invoke(result);
        }

        public Type ReturnType
        {
            get { return typeof (ICollection<object>); }
        }
    }
} // end of namespace

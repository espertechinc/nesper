///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalParamExpr : ExprDotEvalParam
    {
        public ExprDotEvalParamExpr(int parameterNum, ExprNode body, ExprEvaluator bodyEvaluator)
            : base(parameterNum, body, bodyEvaluator)
        {
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.dot
{
    public abstract class ExprDotEvalParam
    {
        protected ExprDotEvalParam(int parameterNum, ExprNode body, ExprEvaluator bodyEvaluator)
        {
            ParameterNum = parameterNum;
            Body = body;
            BodyEvaluator = bodyEvaluator;
        }

        public int ParameterNum { get; private set; }

        public ExprNode Body { get; private set; }

        public ExprEvaluator BodyEvaluator { get; private set; }
    }
}
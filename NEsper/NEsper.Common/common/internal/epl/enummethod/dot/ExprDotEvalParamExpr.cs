///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotEvalParamExpr : ExprDotEvalParam
    {
        public ExprDotEvalParamExpr(
            int parameterNum,
            ExprNode body,
            ExprForge bodyForge)
            : base(
                parameterNum, body, bodyForge)
        {
        }
    }
} // end of namespace
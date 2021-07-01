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
    public abstract class ExprDotEvalParam
    {
        protected ExprDotEvalParam(
            int parameterNum,
            ExprNode body,
            ExprForge bodyForge)
        {
            ParameterNum = parameterNum;
            Body = body;
            BodyForge = bodyForge;
        }

        public int ParameterNum { get; }

        public ExprNode Body { get; }

        public ExprForge BodyForge { get; }
    }
} // end of namespace
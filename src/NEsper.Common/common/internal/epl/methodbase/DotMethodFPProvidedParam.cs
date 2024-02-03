///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public class DotMethodFPProvidedParam
    {
        public DotMethodFPProvidedParam(
            int lambdaParamNum,
            Type returnType,
            ExprNode expression)
        {
            LambdaParamNum = lambdaParamNum;
            ReturnType = returnType;
            Expression = expression;
        }

        public int LambdaParamNum { get; }

        public Type ReturnType { get; }

        public ExprNode Expression { get; }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.methodbase
{
    public class DotMethodFPProvidedParam
    {
        public DotMethodFPProvidedParam(int lambdaParamNum, Type returnType, ExprNode expression)
        {
            LambdaParamNum = lambdaParamNum;
            ReturnType = returnType;
            Expression = expression;
        }

        public int LambdaParamNum { get; private set; }

        public Type ReturnType { get; private set; }

        public ExprNode Expression { get; private set; }
    }
}

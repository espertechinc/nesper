///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprNodeUtilMethodDesc
    {
        public ExprNodeUtilMethodDesc(
            bool allConstants,
            Type[] paramTypes,
            ExprEvaluator[] childEvals,
            MethodInfo reflectionMethod,
            FastMethod fastMethod)
        {
            IsAllConstants = allConstants;
            ParamTypes = paramTypes;
            ChildEvals = childEvals;
            ReflectionMethod = reflectionMethod;
            FastMethod = fastMethod;
        }

        public bool IsAllConstants { get; private set; }

        public Type[] ParamTypes { get; private set; }

        public ExprEvaluator[] ChildEvals { get; private set; }

        public MethodInfo ReflectionMethod { get; private set; }

        public FastMethod FastMethod { get; private set; }
    }
}
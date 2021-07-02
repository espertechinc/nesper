///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilMethodDesc
    {
        public ExprNodeUtilMethodDesc(
            bool allConstants,
            ExprForge[] childForges,
            MethodInfo reflectionMethod,
            Type methodTargetType,
            bool isLocalInlinedClass)
        {
            IsAllConstants = allConstants;
            ChildForges = childForges;
            ReflectionMethod = reflectionMethod;
            MethodTargetType = methodTargetType;
            IsLocalInlinedClass = isLocalInlinedClass;
        }

        public bool IsAllConstants { get; }

        public MethodInfo ReflectionMethod { get; }

        public Type MethodTargetType { get; }

        public ExprForge[] ChildForges { get; }
        
        public bool IsLocalInlinedClass { get; }
    }
} // end of namespace
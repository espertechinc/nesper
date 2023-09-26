///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private readonly bool allConstants;
        private readonly ExprForge[] childForges;
        private readonly MethodInfo reflectionMethod;
        private readonly Type methodTargetType;
        private readonly bool localInlinedClass;

        public ExprNodeUtilMethodDesc(
            bool allConstants,
            ExprForge[] childForges,
            MethodInfo reflectionMethod,
            Type methodTargetType,
            bool localInlinedClass)
        {
            this.allConstants = allConstants;
            this.childForges = childForges;
            this.reflectionMethod = reflectionMethod;
            this.methodTargetType = methodTargetType;
            this.localInlinedClass = localInlinedClass;
        }

        public bool IsAllConstants => allConstants;

        public bool IsLocalInlinedClass => localInlinedClass;

        public MethodInfo ReflectionMethod => reflectionMethod;

        public ExprForge[] ChildForges => childForges;

        public Type MethodTargetType => methodTargetType;
    }
} // end of namespace
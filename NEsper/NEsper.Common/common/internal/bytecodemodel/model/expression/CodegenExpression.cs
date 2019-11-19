///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public interface CodegenExpression
    {
        void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent);

        void MergeClasses(ISet<Type> classes);
    }

    public class CodegenExpressionExtensions
    {
        protected internal static void AssertNonNullArgs(CodegenExpression[] @params)
        {
            for (int i = 0; i < @params.Length; i++) {
                if (@params[i] == null) {
                    throw new ArgumentException("Parameter " + i + " is null");
                }
            }
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionClassMethod : CodegenExpression
    {
        private readonly string _methodName;
        private readonly CodegenExpression[] _params;

        public CodegenExpressionClassMethod(
            string methodName,
            CodegenExpression[] @params)
        {
            _methodName = methodName;
            _params = @params;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append(_methodName);
            builder.Append("(");
            CodegenExpressionBuilder.RenderExpressions(builder, _params, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _params);
        }


        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            CodegenExpressionBuilder.TraverseMultiple(_params, consumer);
        }
    }
} // end of namespace
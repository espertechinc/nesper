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
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenClassUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionNewArrayByLength : CodegenExpression
    {
        private readonly Type component;
        private readonly CodegenExpression expression;

        public CodegenExpressionNewArrayByLength(
            Type component,
            CodegenExpression expression)
        {
            this.component = component;
            this.expression = expression;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            var numDimensions = GetNumberOfDimensions(component);
            var outermostType = GetComponentTypeOutermost(component);
            builder.Append("new ");
            CodeGenerationHelper.AppendClassName(builder, outermostType);
            builder.Append("[");
            expression.Render(builder, isInnerClass, level, indent);
            builder.Append("]");
            for (var i = 0; i < numDimensions; i++) {
                builder.Append("[]");
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(component);
            expression.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(expression);
        }
    }
} // end of namespace
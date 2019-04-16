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
using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenClassUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionNewArrayWithInit : CodegenExpression
    {
        private readonly Type component;
        private readonly CodegenExpression[] expressions;

        public CodegenExpressionNewArrayWithInit(
            Type component,
            CodegenExpression[] expressions)
        {
            this.component = component;
            this.expressions = expressions;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            int numDimensions = GetNumberOfDimensions(component);
            Type outermostType = GetComponentTypeOutermost(component);
            builder.Append("new ");
            CodeGenerationHelper.AppendClassName(builder, outermostType, null, imports);
            builder.Append("[]");
            for (int i = 0; i < numDimensions; i++) {
                builder.Append("[]");
            }

            builder.Append("{");
            CodegenExpressionBuilder.RenderExpressions(builder, expressions, imports, isInnerClass);
            builder.Append("}");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(component);
            foreach (CodegenExpression expression in expressions) {
                expression.MergeClasses(classes);
            }
        }
    }
} // end of namespace
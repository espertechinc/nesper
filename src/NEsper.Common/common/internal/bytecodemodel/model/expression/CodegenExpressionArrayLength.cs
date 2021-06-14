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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionArrayLength : CodegenExpression
    {
        private readonly CodegenExpression _expression;

        public CodegenExpressionArrayLength(CodegenExpression expression)
        {
            _expression = expression;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (_expression is CodegenExpressionRef) {
                _expression.Render(builder, isInnerClass, level, indent);
            }
            else {
                builder.Append("(");
                _expression.Render(builder, isInnerClass, level, indent);
                builder.Append(")");
            }

            builder.Append(".Length");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer) {
            consumer.Invoke(_expression);
        }
    }
} // end of namespace
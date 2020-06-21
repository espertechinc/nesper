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
    public class CodegenExpressionConditional : CodegenExpression
    {
        private readonly CodegenExpression _condition;
        private readonly CodegenExpression _expressionFalse;
        private readonly CodegenExpression _expressionTrue;

        public CodegenExpressionConditional(
            CodegenExpression condition,
            CodegenExpression expressionTrue,
            CodegenExpression expressionFalse)
        {
            _condition = condition;
            _expressionTrue = expressionTrue;
            _expressionFalse = expressionFalse;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("(");
            _condition.Render(builder, isInnerClass, level, indent);
            builder.Append(" ? ");
            _expressionTrue.Render(builder, isInnerClass, level, indent);
            builder.Append(" : ");
            _expressionFalse.Render(builder, isInnerClass, level, indent);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _condition.MergeClasses(classes);
            _expressionTrue.MergeClasses(classes);
            _expressionFalse.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_condition);
            consumer.Invoke(_expressionTrue);
            consumer.Invoke(_expressionFalse);
        }
    }
} // end of namespace
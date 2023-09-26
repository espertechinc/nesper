using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionConcat : CodegenExpression
    {
        private readonly CodegenExpression[] stringExpressions;

        public CodegenExpressionConcat(CodegenExpression[] stringExpressions)
        {
            this.stringExpressions = stringExpressions;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            var first = true;
            foreach (var expression in stringExpressions) {
                if (!first) {
                    builder.Append("+");
                }

                first = false;
                expression.Render(builder, isInnerClass, level, indent);
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            stringExpressions.ForEach(e => e.MergeClasses(classes));
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            CodegenExpressionBuilder.TraverseMultiple(stringExpressions, consumer);
        }
    }
}
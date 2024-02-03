using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionOutputVariable : CodegenExpression
    {
        private readonly Type _variableType;
        private readonly string _variableName;

        public CodegenExpressionOutputVariable(
            Type variableType,
            string variableName)
        {
            _variableType = variableType;
            _variableName = variableName;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("out ");

            if (_variableType != null) {
                CodeGenerationHelper.AppendClassName(builder, _variableType);
                builder.Append(' ');
            }

            builder.Append(_variableName);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_variableType);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            throw new NotSupportedException();
        }
    }
}
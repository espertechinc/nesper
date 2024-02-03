using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public partial class TypeWidenerFactory
    {
        internal class TypeWidenerCompatibleArrayCoercer : TypeWidenerSPI
        {
            private Type inputElementType;
            private Type targetElementType;

            public TypeWidenerCompatibleArrayCoercer(
                Type inputElementType,
                Type targetElementType)
            {
                this.inputElementType = inputElementType;
                this.targetElementType = targetElementType;
                WidenResultType = targetElementType.MakeArrayType();
            }

            public Type WidenResultType { get; }

            public object Widen(object input)
            {
                var inputArray = (Array) input;
                var targetArray = Array.CreateInstance(targetElementType, inputArray.Length);
                for (var ii = 0; ii < inputArray.Length; ii++) {
                    targetArray.SetValue(inputArray.GetValue(ii), ii);
                }

                return targetArray;
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(CompatExtensions),
                    "UnwrapIntoArray",
                    new[] {targetElementType},
                    expression,
                    CodegenExpressionBuilder.ConstantTrue());
            }
        }
    }
}
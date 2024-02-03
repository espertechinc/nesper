using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public partial class TypeWidenerFactory
    {
        private class TypeWidenerDoubleArrayToCollectionCoercer : TypeWidenerSPI
        {
            public Type WidenResultType => typeof(IList<double>);

            public object Widen(object input)
            {
                return input == null ? null : Arrays.AsList((double[])input);
            }

            public CodegenExpression WidenCodegen(
                CodegenExpression expression,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenWidenArrayAsListMayNull(
                    expression,
                    typeof(double[]),
                    codegenMethodScope,
                    typeof(TypeWidenerDoubleArrayToCollectionCoercer),
                    codegenClassScope);
            }
        }
    }
}
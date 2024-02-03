using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public static class FireAndForgetProcessorForgeExtensions
    {
        public static CodegenExpression MakeArray(
            FireAndForgetProcessorForge[] processors,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(FireAndForgetProcessor[]),
                typeof(FireAndForgetProcessorForge),
                classScope);
            method.Block.DeclareVar<FireAndForgetProcessor[]>(
                "processors",
                CodegenExpressionBuilder.NewArrayByLength(
                    typeof(FireAndForgetProcessor),
                    CodegenExpressionBuilder.Constant(processors.Length)));
            for (var i = 0; i < processors.Length; i++) {
                method.Block.AssignArrayElement(
                    "processors",
                    CodegenExpressionBuilder.Constant(i),
                    processors[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(CodegenExpressionBuilder.Ref("processors"));
            return CodegenExpressionBuilder.LocalMethod(method);
        }
    }
}
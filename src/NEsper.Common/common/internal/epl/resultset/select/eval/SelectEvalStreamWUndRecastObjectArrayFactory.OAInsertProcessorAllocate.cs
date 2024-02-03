using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.eval
{
    public partial class SelectEvalStreamWUndRecastObjectArrayFactory
    {
        private class OAInsertProcessorAllocate : SelectExprProcessorForge
        {
            private readonly int underlyingStreamNumber;
            private readonly Item[] items;
            private readonly EventBeanManufacturerForge manufacturer;
            private readonly EventType resultType;

            internal OAInsertProcessorAllocate(
                int underlyingStreamNumber,
                Item[] items,
                EventBeanManufacturerForge manufacturer,
                EventType resultType)
            {
                this.underlyingStreamNumber = underlyingStreamNumber;
                this.items = items;
                this.manufacturer = manufacturer;
                this.resultType = resultType;
            }

            public CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var manufacturerField = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    manufacturer.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
                var refEPS = exprSymbol.GetAddEps(methodNode);
                var block = methodNode.Block.DeclareVar<ObjectArrayBackedEventBean>(
                        "theEvent",
                        Cast(
                            typeof(ObjectArrayBackedEventBean),
                            ArrayAtIndex(
                                refEPS,
                                Constant(underlyingStreamNumber))))
                    .DeclareVar<object[]>(
                        "props",
                        NewArrayByLength(
                            typeof(object),
                            Constant(items.Length)));
                foreach (var item in items) {
                    if (item.OptionalFromIndex != -1) {
                        block.AssignArrayElement(
                            "props",
                            Constant(item.ToIndex),
                            ArrayAtIndex(
                                ExprDotName(
                                    Ref("theEvent"),
                                    "Properties"),
                                Constant(item.OptionalFromIndex)));
                    }
                    else {
                        CodegenExpression value;
                        if (item.OptionalWidener != null) {
                            value = item.Forge.EvaluateCodegen(
                                item.Forge.EvaluationType,
                                methodNode,
                                exprSymbol,
                                codegenClassScope);
                            value = item.OptionalWidener.WidenCodegen(value, methodNode, codegenClassScope);
                        }
                        else {
                            value = item.Forge.EvaluateCodegen(
                                typeof(object),
                                methodNode,
                                exprSymbol,
                                codegenClassScope);
                        }

                        block.AssignArrayElement("props", Constant(item.ToIndex), value);
                    }
                }

                block.MethodReturn(
                    ExprDotMethod(
                        manufacturerField,
                        "Make",
                        Ref("props")));
                return methodNode;
            }

            public EventType ResultEventType => resultType;
        }
    }
}
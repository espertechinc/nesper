///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.typable
{
    public class SelectExprProcessorTypableMultiForge : SelectExprProcessorTypableForge
    {
        private readonly ExprTypableReturnForge typable;
        private readonly bool hasWideners;
        private readonly TypeWidenerSPI[] wideners;
        private readonly EventBeanManufacturerForge factory;
        private readonly EventType targetType;
        private readonly bool firstRowOnly;

        public SelectExprProcessorTypableMultiForge(
            ExprTypableReturnForge typable,
            bool hasWideners,
            TypeWidenerSPI[] wideners,
            EventBeanManufacturerForge factory,
            EventType targetType,
            bool firstRowOnly)
        {
            this.typable = typable;
            this.hasWideners = hasWideners;
            this.wideners = wideners;
            this.factory = factory;
            this.targetType = targetType;
            this.firstRowOnly = firstRowOnly;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (firstRowOnly) {
                var firstMethodNode = codegenMethodScope.MakeChild(
                    typeof(EventBean),
                    typeof(SelectExprProcessorTypableMultiForge),
                    codegenClassScope);
                var firstMethodManufacturer = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    factory.Make(firstMethodNode.Block, codegenMethodScope, codegenClassScope));
                var firstBlock = firstMethodNode.Block.DeclareVar<object[]>(
                        "row",
                        typable.EvaluateTypableSingleCodegen(firstMethodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("row");
                if (hasWideners) {
                    firstBlock.Expression(
                        SelectExprProcessorHelper.ApplyWidenersCodegen(
                            Ref("row"),
                            wideners,
                            firstMethodNode,
                            codegenClassScope));
                }

                firstBlock.MethodReturn(ExprDotMethod(firstMethodManufacturer, "Make", Ref("row")));
                return LocalMethod(firstMethodNode);
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(SelectExprProcessorTypableMultiForge),
                codegenClassScope);

            var methodManufacturer = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventBeanManufacturer),
                factory.Make(methodNode.Block, codegenMethodScope, codegenClassScope));

            var methodBlock = methodNode.Block
                .DeclareVar<object[][]>(
                    "rows",
                    typable.EvaluateTypableMultiCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("rows")
                .IfCondition(EqualsIdentity(ArrayLength(Ref("rows")), Constant(0)))
                .BlockReturn(NewArrayByLength(typeof(EventBean), Constant(0)));
            if (hasWideners) {
                methodBlock.Expression(
                    SelectExprProcessorHelper.ApplyWidenersCodegenMultirow(
                        Ref("rows"),
                        wideners,
                        methodNode,
                        codegenClassScope));
            }

            methodBlock.DeclareVar<EventBean[]>(
                    "events",
                    NewArrayByLength(typeof(EventBean), ArrayLength(Ref("rows"))))
                .ForLoopIntSimple("i", ArrayLength(Ref("events")))
                .AssignArrayElement(
                    "events",
                    Ref("i"),
                    ExprDotMethod(methodManufacturer, "Make", ArrayAtIndex(Ref("rows"), Ref("i"))))
                .BlockEnd()
                .MethodReturn(Ref("events"));
            return LocalMethod(methodNode);
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public Type UnderlyingEvaluationType {
            get {
                if (firstRowOnly) {
                    return targetType.UnderlyingType;
                }

                return TypeHelper.GetArrayType(targetType.UnderlyingType);
            }
        }

        public Type EvaluationType {
            get {
                if (firstRowOnly) {
                    return typeof(EventBean);
                }

                return typeof(EventBean[]);
            }
        }

        public ExprNodeRenderable ExprForgeRenderable => typable.ExprForgeRenderable;
        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace
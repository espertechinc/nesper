///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

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
    public class SelectExprProcessorTypableSingleForge : SelectExprProcessorTypableForge,
        ExprNodeRenderable
    {
        private readonly ExprTypableReturnForge typable;
        private readonly bool hasWideners;
        private readonly TypeWidenerSPI[] wideners;
        private readonly EventBeanManufacturerForge factory;
        private readonly EventType targetType;
        private readonly bool singleRowOnly;

        public SelectExprProcessorTypableSingleForge(
            ExprTypableReturnForge typable,
            bool hasWideners,
            TypeWidenerSPI[] wideners,
            EventBeanManufacturerForge factory,
            EventType targetType,
            bool singleRowOnly)
        {
            this.typable = typable;
            this.hasWideners = hasWideners;
            this.wideners = wideners;
            this.factory = factory;
            this.targetType = targetType;
            this.singleRowOnly = singleRowOnly;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (singleRowOnly) {
                var singleMethodNode = codegenMethodScope.MakeChild(
                    typeof(EventBean),
                    typeof(SelectExprProcessorTypableSingleForge),
                    codegenClassScope);

                var singleMethodManufacturer = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    factory.Make(singleMethodNode.Block, codegenMethodScope, codegenClassScope));

                var singleMethodBlock = singleMethodNode.Block
                    .DeclareVar(
                        typeof(object[]),
                        "row",
                        typable.EvaluateTypableSingleCodegen(singleMethodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("row");
                if (hasWideners) {
                    singleMethodBlock.Expression(
                        SelectExprProcessorHelper.ApplyWidenersCodegen(
                            Ref("row"),
                            wideners,
                            singleMethodNode,
                            codegenClassScope));
                }

                singleMethodBlock.MethodReturn(ExprDotMethod(singleMethodManufacturer, "Make", Ref("row")));
                return LocalMethod(singleMethodNode);
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(SelectExprProcessorTypableSingleForge),
                codegenClassScope);

            var methodManufacturer = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventBeanManufacturer),
                factory.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
            
            var methodBlock = methodNode.Block.DeclareVar(
                    typeof(object[]),
                    "row",
                    typable.EvaluateTypableSingleCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("row");
            if (hasWideners) {
                methodBlock.Expression(
                    SelectExprProcessorHelper.ApplyWidenersCodegen(
                        Ref("row"),
                        wideners,
                        methodNode,
                        codegenClassScope));
            }

            methodBlock
                .DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), Constant(1)))
                .AssignArrayElement("events", Constant(0), ExprDotMethod(methodManufacturer, "Make", Ref("row")))
                .MethodReturn(Ref("events"));
            
            return LocalMethod(methodNode);
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            typable.ExprForgeRenderable.ToEPL(writer, parentPrecedence, flags);
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public Type UnderlyingEvaluationType {
            get {
                if (singleRowOnly) {
                    return targetType.UnderlyingType;
                }

                return TypeHelper.GetArrayType(targetType.UnderlyingType);
            }
        }

        public Type EvaluationType {
            get {
                if (singleRowOnly) {
                    return typeof(EventBean);
                }

                return typeof(EventBean[]);
            }
        }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace
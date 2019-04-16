///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.typable
{
    public class SelectExprProcessorTypableMultiForge : SelectExprProcessorTypableForge
    {
        internal readonly ExprTypableReturnForge typable;
        internal readonly bool hasWideners;
        internal readonly TypeWidenerSPI[] wideners;
        internal readonly EventBeanManufacturerForge factory;
        internal readonly EventType targetType;
        internal readonly bool firstRowOnly;

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

        public ExprEvaluator ExprEvaluator {
            get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField manufacturer = codegenClassScope.AddFieldUnshared(
                true, typeof(EventBeanManufacturer), factory.Make(codegenMethodScope, codegenClassScope));

            if (firstRowOnly) {
                CodegenMethod firstMethodNode = codegenMethodScope
                    .MakeChild(typeof(EventBean), typeof(SelectExprProcessorTypableMultiForge), codegenClassScope);

                CodegenBlock firstBlock = firstMethodNode.Block
                    .DeclareVar(typeof(object[]), "row", typable.EvaluateTypableSingleCodegen(firstMethodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("row");
                if (hasWideners) {
                    firstBlock.Expression(SelectExprProcessorHelper.ApplyWidenersCodegen(@Ref("row"), wideners, firstMethodNode, codegenClassScope));
                }

                firstBlock.MethodReturn(ExprDotMethod(manufacturer, "make", @Ref("row")));
                return LocalMethod(firstMethodNode);
            }

            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]), typeof(SelectExprProcessorTypableMultiForge), codegenClassScope);

            CodegenBlock block = methodNode.Block
                .DeclareVar(typeof(object[][]), "rows", typable.EvaluateTypableMultiCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("rows")
                .IfCondition(EqualsIdentity(ArrayLength(@Ref("rows")), Constant(0)))
                .BlockReturn(NewArrayByLength(typeof(EventBean), Constant(0)));
            if (hasWideners) {
                block.Expression(SelectExprProcessorHelper.ApplyWidenersCodegenMultirow(@Ref("rows"), wideners, methodNode, codegenClassScope));
            }

            block.DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), ArrayLength(@Ref("rows"))))
                .ForLoopIntSimple("i", ArrayLength(@Ref("events")))
                .AssignArrayElement("events", @Ref("i"), ExprDotMethod(manufacturer, "make", ArrayAtIndex(@Ref("rows"), @Ref("i"))))
                .BlockEnd()
                .MethodReturn(@Ref("events"));
            return LocalMethod(methodNode);
        }

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

        public ExprNodeRenderable ForgeRenderable {
            get => typable.ForgeRenderable;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace
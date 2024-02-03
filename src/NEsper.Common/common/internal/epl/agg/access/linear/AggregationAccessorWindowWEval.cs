///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowWEval
    {
        public static void GetValueCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge accessStateFactory,
            AggregationAccessorForgeGetCodegenContext context)
        {
            var size = accessStateFactory.AggregatorLinear.SizeCodegen();
            var enumerator = accessStateFactory.AggregatorLinear.EnumeratorCodegen(
                context.ClassScope,
                context.Method,
                context.NamedMethods);
            var childExpr = CodegenLegoMethodExpression.CodegenExpression(
                forge.ChildNode,
                context.Method,
                context.ClassScope);
            var childExprType = forge.ChildNode.EvaluationType;
            CodegenExpression invokeChild = LocalMethod(
                childExpr,
                Ref("eventsPerStreamBuf"),
                Constant(true),
                Ref(ExprForgeCodegenNames.NAME_EXPREVALCONTEXT));

            var componentType = forge.ComponentType;
            if (componentType != childExprType) {
                invokeChild = Unbox(invokeChild);
            }

            var arrayType = TypeHelper.GetArrayType(componentType);
            context.Method.Block
                .IfCondition(EqualsIdentity(size, Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar(arrayType, "array", NewArrayByLength(componentType, size))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar<IEnumerator<EventBean>>("enumerator", enumerator)
                .DebugStack()
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .AssignArrayElement(Ref("array"), Ref("count"), invokeChild)
                .IncrementRef("count")
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.IfCondition(EqualsIdentity(stateForge.AggregatorLinear.SizeCodegen(), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    stateForge.AggregatorLinear.CollectionReadOnlyCodegen(
                        context.Method,
                        context.ClassScope,
                        context.NamedMethods));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block
                .DeclareVar<int>("size", stateForge.AggregatorLinear.SizeCodegen())
                .IfCondition(EqualsIdentity(Ref("size"), Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar<IList<object>>("values", NewInstance<List<object>>(Ref("size")))
                .DeclareVar<IEnumerator<EventBean>>(
                    "enumerator",
                    stateForge.AggregatorLinear.EnumeratorCodegen(
                        context.ClassScope,
                        context.Method,
                        context.NamedMethods))
                .DebugStack()
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .DeclareVar(
                    forge.ChildNode.EvaluationType.GetBoxedType(),
                    "value",
                    LocalMethod(
                        CodegenLegoMethodExpression.CodegenExpression(
                            forge.ChildNode,
                            context.Method,
                            context.ClassScope),
                        Ref("eventsPerStreamBuf"),
                        ConstantTrue(),
                        Ref(ExprForgeCodegenNames.NAME_EXPREVALCONTEXT)))
                .ExprDotMethod(Ref("values"), "Add", Ref("value"))
                .BlockEnd()
                .MethodReturn(Ref("values"));
        }
    }
} // end of namespace
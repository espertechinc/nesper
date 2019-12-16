///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "first" and "last" aggregation function with index.
    /// </summary>
    public class AggregationAccessorFirstLastIndexWEval
    {
        public static void GetValueCodegen(
            AggregationAccessorFirstLastIndexWEvalForge forge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationStateLinearForge stateForge = (AggregationStateLinearForge) context.AccessStateForge;
            CodegenMethod getBeanFirstLastIndex = GetBeanFirstLastIndexCodegen(
                forge,
                context.Column,
                context.ClassScope,
                stateForge,
                context.Method,
                context.NamedMethods);
            context.Method.Block.DeclareVar<EventBean>("bean", LocalMethod(getBeanFirstLastIndex))
                .DebugStack()
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .MethodReturn(
                    LocalMethod(
                        CodegenLegoMethodExpression.CodegenExpression(
                            forge.ChildNode,
                            context.Method,
                            context.ClassScope,
                            true),
                        Ref("eventsPerStreamBuf"),
                        ConstantTrue(),
                        ConstantNull()));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorFirstLastIndexWEvalForge forge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationStateLinearForge stateForge = (AggregationStateLinearForge) context.AccessStateForge;
            CodegenMethod getBeanFirstLastIndex = GetBeanFirstLastIndexCodegen(
                forge,
                context.Column,
                context.ClassScope,
                stateForge,
                context.Method,
                context.NamedMethods);
            context.Method.Block.DeclareVar<EventBean>("bean", LocalMethod(getBeanFirstLastIndex))
                .IfRefNullReturnNull("bean")
                .MethodReturn(StaticMethod(typeof(Collections), "SingletonList", Ref("bean")));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorFirstLastIndexWEvalForge forge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationStateLinearForge stateForge = (AggregationStateLinearForge) context.AccessStateForge;
            CodegenMethod getBeanFirstLastIndex = GetBeanFirstLastIndexCodegen(
                forge,
                context.Column,
                context.ClassScope,
                stateForge,
                context.Method,
                context.NamedMethods);
            context.Method.Block.DeclareVar<EventBean>("bean", LocalMethod(getBeanFirstLastIndex))
                .DebugStack()
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .DeclareVar<object>(
                    "value",
                    LocalMethod(
                        CodegenLegoMethodExpression.CodegenExpression(
                            forge.ChildNode,
                            context.Method,
                            context.ClassScope,
                            true),
                        Ref("eventsPerStreamBuf"),
                        ConstantTrue(),
                        ConstantNull()))
                .IfRefNullReturnNull("value")
                .MethodReturn(StaticMethod(typeof(Collections), "SingletonList", Ref("value")));
        }

        public static void GetEnumerableEventCodegen(
            AggregationAccessorFirstLastIndexWEvalForge forge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationStateLinearForge stateForge = (AggregationStateLinearForge) context.AccessStateForge;
            CodegenMethod getBeanFirstLastIndex = GetBeanFirstLastIndexCodegen(
                forge,
                context.Column,
                context.ClassScope,
                stateForge,
                context.Method,
                context.NamedMethods);
            context.Method.Block.MethodReturn(LocalMethod(getBeanFirstLastIndex));
        }

        private static CodegenMethod GetBeanFirstLastIndexCodegen(
            AggregationAccessorFirstLastIndexWEvalForge forge,
            int column,
            CodegenClassScope classScope,
            AggregationStateLinearForge stateForge,
            CodegenMethod parent,
            CodegenNamedMethods namedMethods)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(EventBean),
                typeof(AggregationAccessorFirstLastIndexWEval),
                classScope);
            if (forge.Constant == -1) {
                Type evalType = forge.IndexNode.EvaluationType;
                method.Block.DeclareVar(
                    evalType,
                    "indexResult",
                    LocalMethod(
                        CodegenLegoMethodExpression.CodegenExpression(
                            forge.IndexNode,
                            method, 
                            classScope,
                            true),
                        ConstantNull(),
                        ConstantTrue(),
                        ConstantNull()));
                if (evalType.CanBeNull()) {
                    method.Block.IfRefNullReturnNull("indexResult");
                }

                method.Block.DeclareVar<int>(
                    "index",
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("indexResult"), evalType));
            }
            else {
                method.Block.DeclareVar<int>("index", Constant(forge.Constant));
            }

            CodegenExpression value = forge.IsFirst
                ? stateForge.AggregatorLinear.GetFirstNthValueCodegen(Ref("index"), method, classScope, namedMethods)
                : stateForge.AggregatorLinear.GetLastNthValueCodegen(Ref("index"), method, classScope, namedMethods);
            method.Block.MethodReturn(value);
            return method;
        }
    }
} // end of namespace
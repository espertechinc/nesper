///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile {
    public class EnumTakeWhileHelper {
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumcoll">events</param>
        /// <returns>array</returns>
        public static EventBean[] TakeWhileLastEventBeanToArray(ICollection<EventBean> enumcoll)
        {
            return enumcoll.ToArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumcoll">coll</param>
        /// <returns>array</returns>
        public static object[] TakeWhileLastScalarToArray<T>(ICollection<T> enumcoll)
        {
            return enumcoll.Cast<object>().ToArray();
        }

        public static void InitBlockSizeOneScalar(
            int numParameters,
            CodegenBlock block,
            CodegenExpression innerValue,
            Type evaluationType,
            Type itemType)
        {
            var arrayDequeType = typeof(ArrayDeque<>).MakeGenericType(itemType);
            var blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar(itemType, "item", ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First"))
                .AssignArrayElement("props", Constant(0), Ref("item"));
            if (numParameters >= 2) {
                blockSingle.AssignArrayElement("props", Constant(1), Constant(0));
            }

            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                evaluationType,
                innerValue,
                EnumValue(typeof(EmptyList<>).MakeGenericType(itemType), "Instance"));

            blockSingle.BlockReturn(
                StaticMethod(typeof(Collections), "SingletonList", Ref("item")));

            block.DeclareVar(arrayDequeType, "result", NewInstance(arrayDequeType));
        }

        public static void InitBlockSizeOneEvent(
            CodegenBlock block,
            CodegenExpression innerValue,
            int streamNumLambda,
            Type evaluationType)
        {
            var blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar<EventBean>(
                    "item",
                    ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(streamNumLambda), Ref("item"));

            block.DebugStack();

            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                evaluationType,
                innerValue,
                EnumValue(typeof(EmptyList<EventBean>), "Instance"));
            blockSingle.BlockReturn(
                StaticMethod(typeof(Collections), "SingletonList", Ref("item")));

            block.DeclareVar<ArrayDeque<EventBean>>("result", NewInstance<ArrayDeque<EventBean>>());
        }

        public static void InitBlockSizeOneEventPlus(
            int numParameters,
            CodegenBlock block,
            CodegenExpression innerValue,
            int streamNumLambda,
            Type evaluationType)
        {
            var blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar<EventBean>(
                    "item",
                    ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(streamNumLambda), Ref("item"))
                .AssignArrayElement("props", Constant(0), Constant(0));
            if (numParameters > 2) {
                blockSingle.AssignArrayElement("props", Constant(1), Constant(1));
            }

            block.DebugStack();

            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                evaluationType,
                innerValue,
                EnumValue(typeof(EmptyList<EventBean>), "Instance"));

            blockSingle.BlockReturn(
                StaticMethod(typeof(Collections), "SingletonList", Ref("item")));

            block.DeclareVar<ArrayDeque<EventBean>>("result", NewInstance<ArrayDeque<EventBean>>());
        }
    }
} // end of namespace
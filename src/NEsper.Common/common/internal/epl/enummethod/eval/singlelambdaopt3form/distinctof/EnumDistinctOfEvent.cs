///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof {
    public class EnumDistinctOfEvent : ThreeFormEventPlain {
        private readonly Type innerType;

        public EnumDistinctOfEvent(ExprDotEvalParamLambda lambda) : base(lambda)
        {
            innerType = InnerExpression.EvaluationType.GetBoxedType();
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;
                return new ProxyEnumEval()
                {
                    ProcEvaluateEnumMethod = (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        var beans = (ICollection<EventBean>)enumcoll;
                        if (beans.Count <= 1) {
                            return beans;
                        }

                        IDictionary<object, EventBean> distinct = new LinkedHashMap<object, EventBean>();
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var comparable = inner.Evaluate(eventsLambda, isNewData, context);
                            if (!distinct.ContainsKey(comparable)) {
                                distinct.Put(comparable, next);
                            }
                        }

                        return distinct.Values;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return typeof(ICollection<EventBean>);
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return null;
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            block.IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<IDictionary<object, EventBean>>(
                    "distinct",
                    NewInstance(typeof(LinkedHashMap<object, EventBean>)));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            var eval = innerType == null
                ? ConstantNull()
                : InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope);
            EnumDistinctOfHelper.ForEachBlock(block, eval, innerType);
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(ExprDotName(Ref("distinct"), "Values"));
        }
    }
} // end of namespace
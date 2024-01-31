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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby {
    public class EnumOrderByEvent : ThreeFormEventPlain {
        private readonly bool _descending;
        private readonly Type _innerBoxedType;

        public EnumOrderByEvent(
            ExprDotEvalParamLambda lambda,
            bool descending) : base(lambda)
        {
            _descending = descending;
            _innerBoxedType = InnerExpression.EvaluationType.GetBoxedType();
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
                        var sort = new OrderedListDictionary<object, ICollection<EventBean>>();
                        var hasColl = false;

                        var beans = (ICollection<EventBean>)enumcoll;
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var comparable = inner.Evaluate(eventsLambda, isNewData, context);

                            var entry = sort.Get(comparable);
                            if (entry == null) {
                                entry = new ArrayDeque<EventBean>();
                                entry.Add(next);
                                sort[comparable] = entry;

                                continue;
                            }

                            entry.Add(next);
                            hasColl = true;
                        }

                        return EnumOrderByHelper.EnumOrderBySortEval(sort, hasColl, _descending);
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
            block
                .DeclareVar<OrderedListDictionary<object, ICollection<EventBean>>>(
                    "sort",
                    NewInstance(typeof(OrderedListDictionary<object, ICollection<EventBean>>)))
                .DeclareVar<bool>("hasColl", ConstantFalse());
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            EnumOrderByHelper.SortingCode<EventBean>(
                block,
                _innerBoxedType,
                InnerExpression,
                methodNode,
                scope,
                codegenClassScope);
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(
                StaticMethod(
                    typeof(EnumOrderByHelper),
                    "EnumOrderBySortEval",
                    Ref("sort"),
                    Ref("hasColl"),
                    Constant(_descending)));
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby {
    public class EnumOrderByScalar : ThreeFormScalar {
        private readonly bool _descending;
        private readonly Type _innerBoxedType;

        public EnumOrderByScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType,
            int numParameters,
            bool descending) : base(lambda, fieldEventType, numParameters)
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
                        var sort = new OrderedListDictionary<object, ICollection<object>>();
                        var hasColl = false;

                        var resultEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
                        eventsLambda[StreamNumLambda] = resultEvent;
                        var props = resultEvent.Properties;
                        props[2] = enumcoll.Count;
                        var values = enumcoll;

                        var count = -1;
                        foreach (var next in values) {
                            count++;
                            props[1] = count;
                            props[0] = next;

                            var comparable = (IComparable)inner.Evaluate(eventsLambda, isNewData, context);
                            var entry = sort.Get(comparable);
                            if (entry == null) {
                                entry = new ArrayDeque<object>();
                                entry.Add(next);
                                sort.Put(comparable, entry);
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

        public Type KeyType => InnerExpression.EvaluationType ?? typeof(object);

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return desiredReturnType;
            //return typeof(ICollection<>).MakeGenericType(typeof(object));
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return null;
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            var keyType = KeyType;
            var valType = desiredReturnType;
            var dictType = typeof(IOrderedDictionary<,>).MakeGenericType(keyType, valType);
            var implType = typeof(OrderedListDictionary<,>).MakeGenericType(keyType, valType);

            block
                .DeclareVar(dictType, "sort", NewInstance(implType))
                .DeclareVar<bool>("hasColl", ConstantFalse());
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType)
        {
            EnumOrderByHelper.SortingCode(
                desiredReturnType.GetComponentType(),
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
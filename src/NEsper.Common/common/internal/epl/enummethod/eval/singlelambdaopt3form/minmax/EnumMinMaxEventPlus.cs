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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmax {
    public class EnumMinMaxEventPlus : ThreeFormEventPlus {
        private readonly bool _max;
        private readonly Type _innerTypeBoxed;

        public EnumMinMaxEventPlus(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType indexEventType,
            int numParameters,
            bool max) : base(lambda, indexEventType, numParameters)
        {
            _max = max;
            _innerTypeBoxed = InnerExpression.EvaluationType.GetBoxedType();
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
                        IComparable minKey = null;
                        var beans = (ICollection<EventBean>)enumcoll;
                        var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
                        var props = indexEvent.Properties;
                        props[1] = enumcoll.Count;
                        eventsLambda[StreamNumLambda + 1] = indexEvent;
                        var count = -1;

                        foreach (var next in beans) {
                            count++;
                            props[0] = count;
                            eventsLambda[StreamNumLambda] = next;

                            var comparable = inner.Evaluate(eventsLambda, isNewData, context);
                            if (comparable == null) {
                                continue;
                            }

                            if (minKey == null) {
                                minKey = (IComparable)comparable;
                            }
                            else {
                                if (_max) {
                                    if (minKey.CompareTo(comparable) < 0) {
                                        minKey = (IComparable)comparable;
                                    }
                                }
                                else {
                                    if (minKey.CompareTo(comparable) > 0) {
                                        minKey = (IComparable)comparable;
                                    }
                                }
                            }
                        }

                        return minKey;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return _innerTypeBoxed;
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return ConstantNull();
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            block.DeclareVar(_innerTypeBoxed, "minKey", ConstantNull());
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            block.DeclareVar(
                _innerTypeBoxed,
                "value",
                InnerExpression.EvaluateCodegen(_innerTypeBoxed, methodNode, scope, codegenClassScope));
            if (!InnerExpression.EvaluationType.IsPrimitive) {
                block.IfRefNull("value").BlockContinue();
            }

            block
                .IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Ref("value"))
                .IfElse()
                .IfCondition(
                    Relational(ExprDotMethod(Ref("minKey"), "CompareTo", Ref("value")), _max ? LT : GT, Constant(0)))
                .AssignRef("minKey", Ref("value"));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("minKey"));
        }
    }
} // end of namespace
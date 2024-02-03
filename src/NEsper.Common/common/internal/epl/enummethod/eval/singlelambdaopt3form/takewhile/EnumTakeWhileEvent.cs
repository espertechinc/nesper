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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile {
    public class EnumTakeWhileEvent : ThreeFormEventPlain {
        private CodegenExpression _innerValue;

        public EnumTakeWhileEvent(ExprDotEvalParamLambda lambda) : base(lambda)
        {
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
                        if (enumcoll.IsEmpty()) {
                            return enumcoll;
                        }

                        var beans = (ICollection<EventBean>)enumcoll;
                        if (enumcoll.Count == 1) {
                            var item = beans.First();
                            eventsLambda[StreamNumLambda] = item;

                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                return EmptyList<EventBean>.Instance;
                            }

                            return Collections.SingletonList<EventBean>(item);
                        }

                        var result = new ArrayDeque<EventBean>();

                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (pass == null || false.Equals(pass)) {
                                break;
                            }

                            result.Add(next);
                        }

                        return result;
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
            //return EnumForgeCodegenNames.REF_ENUMCOLL;
            return EnumValue(typeof(EmptyList<EventBean>), "Instance");
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            _innerValue = InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);
            EnumTakeWhileHelper.InitBlockSizeOneEvent(
                block,
                _innerValue,
                StreamNumLambda,
                InnerExpression.EvaluationType);
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                block,
                InnerExpression.EvaluationType,
                _innerValue);
            block.Expression(ExprDotMethod(Ref("result"), "Add", Ref("next")));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace
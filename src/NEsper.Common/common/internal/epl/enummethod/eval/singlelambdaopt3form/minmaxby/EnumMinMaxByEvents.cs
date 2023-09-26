///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmaxby
{
    public class EnumMinMaxByEvents : ThreeFormEventPlain
    {
        private readonly bool _max;
        private readonly Type _innerTypeBoxed;

        public EnumMinMaxByEvents(
            ExprDotEvalParamLambda lambda,
            bool max) : base(lambda)
        {
            _max = max;
            _innerTypeBoxed = InnerExpression.EvaluationType.GetBoxedType();
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;
                return new ProxyEnumEval() {
                    ProcEvaluateEnumMethod = (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        IComparable minKey = null;
                        EventBean result = null;

                        var beans = (ICollection<EventBean>)enumcoll;
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var comparable = inner.Evaluate(eventsLambda, isNewData, context);
                            if (comparable == null) {
                                continue;
                            }

                            if (minKey == null) {
                                minKey = (IComparable)comparable;
                                result = next;
                            }
                            else {
                                if (_max) {
                                    if (minKey.CompareTo(comparable) < 0) {
                                        minKey = (IComparable)comparable;
                                        result = next;
                                    }
                                }
                                else {
                                    if (minKey.CompareTo(comparable) > 0) {
                                        minKey = (IComparable)comparable;
                                        result = next;
                                    }
                                }
                            }
                        }

                        return result;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod()
        {
            return typeof(EventBean);
        }

        public override CodegenExpression ReturnIfEmptyOptional()
        {
            return ConstantNull();
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar(_innerTypeBoxed, "minKey", ConstantNull())
                .DeclareVar<EventBean>("result", ConstantNull());
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar(
                    _innerTypeBoxed,
                    "value",
                    InnerExpression.EvaluateCodegen(_innerTypeBoxed, methodNode, scope, codegenClassScope))
                .IfRefNull("value")
                .BlockContinue()
                .IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Ref("value"))
                .AssignRef("result", Ref("next"))
                .IfElse()
                .IfCondition(
                    Relational(ExprDotMethod(Ref("minKey"), "CompareTo", Ref("value")), _max ? LT : GT, Constant(0)))
                .AssignRef("minKey", Ref("value"))
                .AssignRef("result", Ref("next"));
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Ref("result"));
        }
    }
} // end of namespace
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.allofanyof {
    public class EnumAllOfAnyOfEvent : ThreeFormEventPlain {
        private readonly bool _all;

        public EnumAllOfAnyOfEvent(
            ExprDotEvalParamLambda lambda,
            bool all) : base(lambda)
        {
            _all = all;
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
                            return _all;
                        }

                        var beans = (ICollection<EventBean>)enumcoll;
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var pass = inner.Evaluate(eventsLambda, isNewData, context);
                            if (_all) {
                                if (pass == null || false.Equals(pass)) {
                                    return false;
                                }
                            }
                            else {
                                if (pass != null && (bool)pass) {
                                    return true;
                                }
                            }
                        }

                        return _all;
                    }
                };
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return typeof(bool);
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return Constant(_all);
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            CodegenLegoBooleanExpression.CodegenReturnBoolIfNullOrBool(
                block,
                InnerExpression.EvaluationType,
                InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope),
                _all,
                _all ? false : (bool?)null,
                !_all,
                !_all);
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.MethodReturn(Constant(_all));
        }
    }
} // end of namespace
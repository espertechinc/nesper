///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprEventIdentityEqualsNodeEval : ExprEvaluator
    {
        private readonly int _streamLeft;
        private readonly int _streamRight;

        public ExprEventIdentityEqualsNodeEval(
            int streamLeft,
            int streamRight)
        {
            this._streamLeft = streamLeft;
            this._streamRight = streamRight;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var left = eventsPerStream[_streamLeft];
            var right = eventsPerStream[_streamRight];
            if (left == null || right == null) {
                return null;
            }

            return left.Equals(right);
        }

        public static CodegenExpression EvaluateCodegen(
            ExprEventIdentityEqualsNodeForge forge,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool?), typeof(ExprEventIdentityEqualsNodeEval), classScope);
            method.Block
                .DeclareVar<EventBean>(
                    "left",
                    ArrayAtIndex(symbols.GetAddEps(method), Constant(forge.UndLeft.StreamId)))
                .DeclareVar<EventBean>(
                    "right",
                    ArrayAtIndex(symbols.GetAddEps(method), Constant(forge.UndRight.StreamId)))
                .IfCondition(Or(EqualsNull(Ref("left")), EqualsNull(Ref("right"))))
                .BlockReturn(ConstantNull())
                .MethodReturn(StaticMethod<object>("Equals", Ref("left"), Ref("right")));
            return LocalMethod(method);
        }
    }
} // end of namespace
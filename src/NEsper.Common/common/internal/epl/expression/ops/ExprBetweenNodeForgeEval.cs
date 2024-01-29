///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprBetweenNodeForgeEval : ExprEvaluator
    {
        private readonly ExprBetweenNodeForge _forge;
        private readonly ExprEvaluator _higherEval;
        private readonly ExprEvaluator _lowerEval;
        private readonly ExprEvaluator _valueEval;

        public ExprBetweenNodeForgeEval(
            ExprBetweenNodeForge forge,
            ExprEvaluator valueEval,
            ExprEvaluator lowerEval,
            ExprEvaluator higherEval)
        {
            _forge = forge;
            _valueEval = valueEval;
            _lowerEval = lowerEval;
            _higherEval = higherEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Evaluate first child which is the base value to compare to
            var value = _valueEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (value == null) {
                return false;
            }

            var lower = _lowerEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var higher = _higherEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            var result = _forge.Computer.IsBetween(value, lower, higher);
            result = result ^ _forge.ForgeRenderable.IsNotBetween;

            return result;
        }

        public static CodegenExpression Codegen(
            ExprBetweenNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var nodes = forge.ForgeRenderable.ChildNodes;
            var value = nodes[0].Forge;
            var lower = nodes[1].Forge;
            var higher = nodes[2].Forge;
            var isNot = forge.ForgeRenderable.IsNotBetween;

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool),
                typeof(ExprBetweenNodeForgeEval),
                codegenClassScope);
            var block = methodNode.Block;

            var valueType = value.EvaluationType.GetBoxedType();
            block.DeclareVar(
                valueType,
                "value",
                value.EvaluateCodegen(valueType, methodNode, exprSymbol, codegenClassScope));
            if (valueType.CanBeNull()) {
                block.IfRefNullReturnFalse("value");
            }

            var lowerType = lower.EvaluationType.GetBoxedType();
            block.DeclareVar(
                lowerType,
                "lower",
                lower.EvaluateCodegen(lowerType, methodNode, exprSymbol, codegenClassScope));
            if (lowerType.CanBeNull()) {
                block.IfRefNull("lower").BlockReturn(Constant(isNot));
            }

            var higherType = higher.EvaluationType.GetBoxedType();
            block.DeclareVar(
                higherType,
                "higher",
                higher.EvaluateCodegen(higherType, methodNode, exprSymbol, codegenClassScope));
            if (higher.EvaluationType.CanBeNull()) {
                block.IfRefNull("higher").BlockReturn(Constant(isNot));
            }

            block.DeclareVar<bool>(
                "result",
                forge.Computer.CodegenNoNullCheck(
                    Unbox(Ref("value"), valueType),
                    valueType.GetUnboxedType(),
                    Unbox(Ref("lower"), lowerType),
                    lowerType.GetUnboxedType(),
                    Unbox(Ref("higher"), higherType),
                    higherType.GetUnboxedType(),
                    methodNode,
                    codegenClassScope));

            block.MethodReturn(NotOptional(forge.ForgeRenderable.IsNotBetween, Ref("result")));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace
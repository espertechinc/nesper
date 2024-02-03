///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;


namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    /// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
    /// </summary>
    public class ExprMinMaxRowNodeForgeEval : ExprEvaluator
    {
        private readonly ExprMinMaxRowNodeForge _forge;
        private readonly MinMaxTypeEnumComputer _computer;

        public ExprMinMaxRowNodeForgeEval(
            ExprMinMaxRowNodeForge forge,
            ExprEvaluator[] evaluators,
            ExprForge[] forges)
        {
            _forge = forge;
            if (forge.EvaluationType == typeof(BigInteger)) {
                var convertors = new BigIntegerCoercer[evaluators.Length];
                for (var i = 0; i < evaluators.Length; i++) {
                    convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(forges[i].EvaluationType);
                }

                _computer = new MinMaxTypeEnumComputer.MinMaxComputerBigIntCoerce(
                    evaluators,
                    convertors,
                    forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX);
            }
            else if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                _computer = new MinMaxTypeEnumComputer.MaxComputerDoubleCoerce(evaluators);
            }
            else {
                _computer = new MinMaxTypeEnumComputer.MinComputerDoubleCoerce(evaluators);
            }
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = _computer.Execute(eventsPerStream, isNewData, exprEvaluatorContext);
            if (result == null) {
                return null;
            }

            return TypeHelper.CoerceBoxed(result, _forge.EvaluationType);
        }

        public static CodegenExpression Codegen(
            ExprMinMaxRowNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var resultType = forge.EvaluationType;
            var nodes = forge.ForgeRenderable.ChildNodes;

            CodegenExpression expression;
            if (resultType == typeof(BigInteger)) {
                var convertors = new BigIntegerCoercer[nodes.Length];
                for (var i = 0; i < nodes.Length; i++) {
                    convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(nodes[i].Forge.EvaluationType);
                }

                expression = MinMaxTypeEnumComputer.MinMaxComputerBigIntCoerce.Codegen(
                    forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    nodes,
                    convertors);
            }
            else if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                expression = MinMaxTypeEnumComputer.MaxComputerDoubleCoerce.Codegen(
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    nodes,
                    resultType);
            }
            else {
                expression = MinMaxTypeEnumComputer.MinComputerDoubleCoerce.Codegen(
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    nodes,
                    resultType);
            }

            return expression;
        }
    }
} // end of namespace
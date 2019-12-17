///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
    /// </summary>
    public class ExprMinMaxRowNodeForgeEval : ExprEvaluator
    {
        private readonly MinMaxType.Computer computer;

        private readonly ExprMinMaxRowNodeForge forge;

        public ExprMinMaxRowNodeForgeEval(
            ExprMinMaxRowNodeForge forge,
            ExprEvaluator[] evaluators,
            ExprForge[] forges)
        {
            this.forge = forge;
            if (forge.EvaluationType.IsBigInteger()) {
                var convertors = new BigIntegerCoercer[evaluators.Length];
                for (var i = 0; i < evaluators.Length; i++) {
                    convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(forges[i].EvaluationType);
                }

                computer = new MinMaxType.ComputerBigIntCoerce(
                    evaluators,
                    convertors,
                    forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX);
            }
            else if (forge.EvaluationType.IsDecimal()) {
                if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                    computer = new MinMaxType.MaxComputerDecimalCoerce(evaluators);
                }
                else {
                    computer = new MinMaxType.MinComputerDecimalCoerce(evaluators);
                }
            }
            else {
                if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                    computer = new MinMaxType.MaxComputerDoubleCoerce(evaluators);
                }
                else {
                    computer = new MinMaxType.MinComputerDoubleCoerce(evaluators);
                }
            }
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = computer.Execute(eventsPerStream, isNewData, exprEvaluatorContext);
            if (result == null) {
                return null;
            }

            return TypeHelper.CoerceBoxed(result, forge.EvaluationType);
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
            if (resultType.IsBigInteger()) {
                var convertors = new BigIntegerCoercer[nodes.Length];
                for (var i = 0; i < nodes.Length; i++) {
                    convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(nodes[i].Forge.EvaluationType);
                }

                expression = MinMaxType.ComputerBigIntCoerce.Codegen(
                    forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    nodes,
                    convertors);
            }
            else if (resultType.IsDecimal()) {
                if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                    expression = MinMaxType.MaxComputerDecimalCoerce.Codegen(
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope,
                        nodes,
                        resultType);
                }
                else {
                    expression = MinMaxType.MinComputerDecimalCoerce.Codegen(
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope,
                        nodes,
                        resultType);
                }
            }
            else {
                if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                    expression = MinMaxType.MaxComputerDoubleCoerce.Codegen(
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope,
                        nodes,
                        resultType);
                }
                else {
                    expression = MinMaxType.MinComputerDoubleCoerce.Codegen(
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope,
                        nodes,
                        resultType);
                }
            }

            return expression;
        }
    }
} // end of namespace
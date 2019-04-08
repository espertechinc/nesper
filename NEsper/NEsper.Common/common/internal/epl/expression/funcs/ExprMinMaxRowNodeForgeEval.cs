///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
	/// <summary>
	/// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
	/// </summary>
	public class ExprMinMaxRowNodeForgeEval : ExprEvaluator {

	    private readonly ExprMinMaxRowNodeForge forge;
	    private readonly MinMaxTypeEnum.Computer computer;

	    public ExprMinMaxRowNodeForgeEval(ExprMinMaxRowNodeForge forge, ExprEvaluator[] evaluators, ExprForge[] forges) {
	        this.forge = forge;
	        if (forge.EvaluationType == typeof(BigInteger)) {
	            BigIntegerCoercer[] convertors = new BigIntegerCoercer[evaluators.Length];
	            for (int i = 0; i < evaluators.Length; i++) {
	                convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(forges[i].EvaluationType);
	            }
	            computer = new MinMaxTypeEnum.ComputerBigIntCoerce(evaluators, convertors, forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX);
	        } else if (forge.EvaluationType == typeof(BigDecimal)) {
	            SimpleNumberDecimalCoercer[] convertors = new SimpleNumberDecimalCoercer[evaluators.Length];
	            for (int i = 0; i < evaluators.Length; i++) {
	                convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigDecimal(forges[i].EvaluationType);
	            }
	            computer = new MinMaxTypeEnum.ComputerBigDecCoerce(evaluators, convertors, forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX);
	        } else {
	            if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
	                computer = new MinMaxTypeEnum.MaxComputerDoubleCoerce(evaluators);
	            } else {
	                computer = new MinMaxTypeEnum.MinComputerDoubleCoerce(evaluators);
	            }
	        }
	    }

	    public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        Number result = computer.Execute(eventsPerStream, isNewData, exprEvaluatorContext);
	        if (result == null) {
	            return null;
	        }
	        return TypeHelper.CoerceBoxed(result, forge.EvaluationType);
	    }

	    public static CodegenExpression Codegen(ExprMinMaxRowNodeForge forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        Type resultType = forge.EvaluationType;
	        ExprNode[] nodes = forge.ForgeRenderable.ChildNodes;

	        CodegenExpression expression;
	        if (resultType == typeof(BigInteger)) {
	            BigIntegerCoercer[] convertors = new BigIntegerCoercer[nodes.Length];
	            for (int i = 0; i < nodes.Length; i++) {
	                convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigInteger(nodes[i].Forge.EvaluationType);
	            }
	            expression = MinMaxTypeEnum.ComputerBigIntCoerce.Codegen(forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX, codegenMethodScope, exprSymbol, codegenClassScope, nodes, convertors);
	        } else if (resultType == typeof(BigDecimal)) {
	            SimpleNumberDecimalCoercer[] convertors = new SimpleNumberDecimalCoercer[nodes.Length];
	            for (int i = 0; i < nodes.Length; i++) {
	                convertors[i] = SimpleNumberCoercerFactory.GetCoercerBigDecimal(nodes[i].Forge.EvaluationType);
	            }
	            expression = MinMaxTypeEnum.ComputerBigDecCoerce.Codegen(forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX, codegenMethodScope, exprSymbol, codegenClassScope, nodes, convertors);
	        } else {
	            if (forge.ForgeRenderable.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
	                expression = MinMaxTypeEnum.MaxComputerDoubleCoerce.Codegen(codegenMethodScope, exprSymbol, codegenClassScope, nodes, resultType);
	            } else {
	                expression = MinMaxTypeEnum.MinComputerDoubleCoerce.Codegen(codegenMethodScope, exprSymbol, codegenClassScope, nodes, resultType);
	            }
	        }
	        return expression;
	    }

	}
} // end of namespace
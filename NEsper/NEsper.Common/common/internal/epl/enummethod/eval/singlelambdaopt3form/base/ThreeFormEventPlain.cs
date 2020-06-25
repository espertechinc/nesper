///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public abstract class ThreeFormEventPlain : EnumForgeBasePlain
	{
		public abstract Type ReturnType();

		public abstract CodegenExpression ReturnIfEmptyOptional();

		public abstract void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope);

		public virtual bool HasForEachLoop()
		{
			return true;
		}

		public abstract void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope);

		public abstract void ReturnResult(CodegenBlock block);

		public ThreeFormEventPlain(ExprDotEvalParamLambda lambda) : base(lambda)
		{
		}

		public CodegenExpression Codegen(
			EnumForgeCodegenParams premade,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
			CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(ReturnType(), GetType(), scope, codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);
			CodegenBlock block = methodNode.Block;

			CodegenExpression returnEmpty = ReturnIfEmptyOptional();
			if (returnEmpty != null) {
				block.IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
					.BlockReturn(returnEmpty);
			}

			InitBlock(block, methodNode, scope, codegenClassScope);

			if (HasForEachLoop()) {
				CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
					.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), @Ref("next"));
				ForEachBlock(forEach, methodNode, scope, codegenClassScope);
			}

			ReturnResult(block);
			return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
		}
	}
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.filter
{
	public class ExprFilterReboolValueNode : ExprNodeBase,
		ExprForge
	{
		private readonly Type returnType;

		public ExprFilterReboolValueNode(Type returnType)
		{
			this.returnType = returnType;
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			writer.Write("?");
		}

		public override ExprPrecedenceEnum Precedence {
			get { return ExprPrecedenceEnum.UNARY; }
		}

		public override bool EqualsNode(
			ExprNode node,
			bool ignoreStreamPrefix)
		{
			throw new UnsupportedOperationException("Compare is not available");
		}

		public override ExprForge Forge {
			get { return this; }
		}

		public override ExprNode Validate(ExprValidationContext validationContext)
		{
			// nothing to validate
			return null;
		}

		public ExprEvaluator ExprEvaluator {
			get { throw new UnsupportedOperationException("Evaluator not available"); }
		}

		public CodegenExpression EvaluateCodegen(
			Type requiredType,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			return Cast(
				requiredType,
				CodegenLegoCast.CastSafeFromObjectType(requiredType,
					ExprDotName(symbols.GetAddExprEvalCtx(parent), "FilterReboolConstant")));
		}

		public Type EvaluationType {
			get { return returnType; }
		}

		public ExprForgeConstantType ForgeConstantType {
			get { return ExprForgeConstantType.NONCONST; }
		}

		public ExprNodeRenderable ExprForgeRenderable {
			get {
				return new ProxyExprNodeRenderable() {
					ProcToEPL = (
						writer,
						parentPrecedence,
						flags) => {
						writer.Write(typeof(ExprFilterReboolValueNode).Name);
						writer.Write("(");
						writer.Write(returnType.GetType().Name);
						writer.Write(")");
					},
				};
			}
		}
	}
} // end of namespace

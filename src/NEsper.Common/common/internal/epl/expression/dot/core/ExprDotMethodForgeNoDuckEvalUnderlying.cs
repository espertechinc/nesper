///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotMethodForgeNoDuckEvalUnderlying : ExprDotMethodForgeNoDuckEvalPlain
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public ExprDotMethodForgeNoDuckEvalUnderlying(
			ExprDotMethodForgeNoDuck forge,
			ExprEvaluator[] parameters) : base(forge, parameters)
		{
		}

		public override object Evaluate(
			object target,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			if (target == null) {
				return null;
			}

			if (!(target is EventBean)) {
				Log.Warn("Expected EventBean return value but received '" + target.GetType().Name + "' for statement " + forge.OptionalStatementName);
				return null;
			}

			var bean = (EventBean) target;
			return base.Evaluate(bean.Underlying, eventsPerStream, isNewData, exprEvaluatorContext);
		}

		public static CodegenExpression CodegenUnderlying(
			ExprDotMethodForgeNoDuck forge,
			CodegenExpression inner,
			Type innerType,
			CodegenMethodScope codegenMethodScope,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			var underlyingType = forge.Method.DeclaringType;
			var returnType = forge.Method.ReturnType;
			var methodNode = codegenMethodScope
				.MakeChild(returnType.GetBoxedType(), typeof(ExprDotMethodForgeNoDuckEvalUnderlying), codegenClassScope)
				.AddParam(typeof(EventBean), "target");

			var eval = ExprDotMethodForgeNoDuckEvalPlain.CodegenPlain(
				forge,
				Ref("underlying"),
				underlyingType,
				methodNode,
				exprSymbol,
				codegenClassScope);
			
			if (returnType != typeof(void)) {
				methodNode.Block
					.IfRefNullReturnNull("target")
					.DeclareVar(underlyingType, "underlying", Cast(underlyingType, ExprDotName(Ref("target"), "Underlying")))
					.MethodReturn(eval);
			}
			else {
				methodNode.Block
					.IfRefNotNull("target")
					.DeclareVar(underlyingType, "underlying", Cast(underlyingType, ExprDotName(Ref("target"), "Underlying")))
					.Expression(eval);
			}

			return LocalMethod(methodNode, inner);
		}
	}
} // end of namespace

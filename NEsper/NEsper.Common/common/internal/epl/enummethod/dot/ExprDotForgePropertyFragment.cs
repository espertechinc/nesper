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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
	public class ExprDotForgePropertyFragment : ExprDotEval,
		ExprDotForge
	{

		private readonly EventPropertyGetterSPI _getter;
		private readonly EPType _returnType;

		public ExprDotForgePropertyFragment(
			EventPropertyGetterSPI getter,
			EPType returnType)
		{
			_getter = getter;
			_returnType = returnType;
		}

		public object Evaluate(
			object target,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			if (!(target is EventBean)) {
				return null;
			}

			return _getter.GetFragment((EventBean) target);
		}

		public EPType TypeInfo => _returnType;

		public void Visit(ExprDotEvalVisitor visitor)
		{
			visitor.VisitPropertySource();
		}

		public ExprDotEval DotEvaluator => this;

		public ExprDotForge DotForge => this;

		public CodegenExpression Codegen(
			CodegenExpression inner,
			Type innerType,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			var type = EPTypeHelper.GetCodegenReturnType(_returnType);
			if (innerType == typeof(EventBean)) {
				return CodegenLegoCast.CastSafeFromObjectType(type, _getter.EventBeanFragmentCodegen(inner, parent, classScope));
			}

			var methodNode = parent.MakeChild(type, typeof(ExprDotForgePropertyFragment), classScope).AddParam(innerType, "target");

			methodNode.Block
				.IfInstanceOf("target", typeof(EventBean))
				.BlockReturn(
					CodegenLegoCast.CastSafeFromObjectType(type, _getter.EventBeanFragmentCodegen(Cast(typeof(EventBean), inner), methodNode, classScope)))
				.MethodReturn(ConstantNull());
			return LocalMethod(methodNode, inner);
		}
	}
} // end of namespace

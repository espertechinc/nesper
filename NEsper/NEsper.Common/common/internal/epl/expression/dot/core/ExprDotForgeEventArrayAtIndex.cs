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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
	public class ExprDotForgeEventArrayAtIndex : ExprDotForge
	{
		private readonly EPType _returnType;
		private readonly ExprNode _indexExpression;

		public ExprDotForgeEventArrayAtIndex(
			EPType returnType,
			ExprNode indexExpression)
		{
			_returnType = returnType;
			_indexExpression = indexExpression;
		}

		public EPType TypeInfo => _returnType;

		public void Visit(ExprDotEvalVisitor visitor)
		{
			visitor.VisitArraySingleItemSource();
		}

		public ExprDotEval DotEvaluator {
			get {
				return new ProxyExprDotEval() {
					ProcEvaluate = (
						target,
						eventsPerStream,
						isNewData,
						exprEvaluatorContext) => {
						EventBean[] events = (EventBean[]) target;
						if (events == null) {
							return null;
						}

						int? index = _indexExpression.Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext).AsBoxedInt32();
						if (index == null) {
							return null;
						}

						return events[index.Value];
					},

					ProcDotForge = () => this,
				};
			}
		}

		public CodegenExpression Codegen(
			CodegenExpression inner,
			Type innerType,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenMethod method = parent.MakeChild(typeof(EventBean), typeof(ExprDotForgeProperty), classScope)
				.AddParam(typeof(EventBean[]), "target")
				.AddParam(typeof(int?), "index");
			method.Block
				.IfNullReturnNull(Ref("target"))
				.IfCondition(Relational(Ref("index"), CodegenExpressionRelational.CodegenRelational.GE, ArrayLength(Ref("target"))))
				.BlockThrow(
					NewInstance(
						typeof(EPException),
						Concat(Constant("Array length "), ArrayLength(Ref("target")), Constant(" less than index "), Ref("index"))))
				.MethodReturn(ArrayAtIndex(Ref("target"), Cast(typeof(int), Ref("index"))));
			return LocalMethod(method, inner, _indexExpression.Forge.EvaluateCodegen(typeof(int?), method, symbols, classScope));
		}
	}
} // end of namespace

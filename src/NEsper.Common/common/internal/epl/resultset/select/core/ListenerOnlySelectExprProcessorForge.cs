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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

//GETSTATEMENTRESULTSERVICE

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
	public class ListenerOnlySelectExprProcessorForge : SelectExprProcessorForge
	{
		private readonly SelectExprProcessorForge syntheticProcessorForge;

		public ListenerOnlySelectExprProcessorForge(SelectExprProcessorForge syntheticProcessorForge)
		{
			this.syntheticProcessorForge = syntheticProcessorForge;
		}

		public EventType ResultEventType => syntheticProcessorForge.ResultEventType;

		public CodegenMethod ProcessCodegen(
			CodegenExpression resultEventType,
			CodegenExpression eventBeanFactory,
			CodegenMethodScope codegenMethodScope,
			SelectExprProcessorCodegenSymbol selectSymbol,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			var processMethod = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);

			var isSythesize = selectSymbol.GetAddSynthesize(processMethod);
			var syntheticMethod = syntheticProcessorForge.ProcessCodegen(
				resultEventType,
				eventBeanFactory,
				processMethod,
				selectSymbol,
				exprSymbol,
				codegenClassScope);

			var stmtResultSvc = codegenClassScope.AddDefaultFieldUnshared(
				true,
				typeof(StatementResultService),
				ExprDotName(EPStatementInitServicesConstants.REF, EPStatementInitServicesConstants.STATEMENTRESULTSERVICE));
			processMethod.Block
				.IfCondition(Or(isSythesize, ExprDotName(stmtResultSvc, "IsMakeSynthetic")))
				.BlockReturn(LocalMethod(syntheticMethod))
				.MethodReturn(ConstantNull());

			return processMethod;
		}
	}
} // end of namespace

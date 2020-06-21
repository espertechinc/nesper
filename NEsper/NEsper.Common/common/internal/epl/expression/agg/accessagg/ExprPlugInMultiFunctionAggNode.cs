///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.plugin;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
	/// <summary>
	/// Represents a custom aggregation function in an expresson tree.
	/// </summary>
	public class ExprPlugInMultiFunctionAggNode : ExprAggregateNodeBase,
		ExprEnumerationEval,
		ExprAggMultiFunctionNode,
		ExprPlugInAggNodeMarker
	{
		private readonly AggregationMultiFunctionForge _aggregationMultiFunctionForge;
		private readonly string _functionName;
		private readonly ConfigurationCompilerPlugInAggregationMultiFunction _config;
		private AggregationForgeFactoryAccessPlugin _factory;

		public ExprPlugInMultiFunctionAggNode(
			bool distinct,
			ConfigurationCompilerPlugInAggregationMultiFunction config,
			AggregationMultiFunctionForge aggregationMultiFunctionForge,
			string functionName)
			: base(distinct)
		{
			this._aggregationMultiFunctionForge = aggregationMultiFunctionForge;
			this._functionName = functionName;
			this._config = config;
		}

		public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
		{
			ValidatePositionals(validationContext);
			// validate using the context provided by the 'outside' streams to determine parameters
			// at this time 'inside' expressions like 'window(intPrimitive)' are not handled
			ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, this.ChildNodes, validationContext);
			AggregationMultiFunctionValidationContext ctx = new AggregationMultiFunctionValidationContext(
				_functionName,
				validationContext.StreamTypeService.EventTypes,
				positionalParams,
				validationContext.StatementName,
				validationContext,
				_config,
				ChildNodes,
				optionalFilter);
			AggregationMultiFunctionHandler handlerPlugin = _aggregationMultiFunctionForge.ValidateGetHandler(ctx);
			_factory = new AggregationForgeFactoryAccessPlugin(this, handlerPlugin);
			return _factory;
		}

		public override string AggregationFunctionName => _functionName;

		public override bool IsFilterExpressionAsLastParameter => false;

		public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
		{
			return false;
		}

		public Type ComponentTypeCollection => _factory.ComponentTypeCollection;

		public EventType GetEventTypeCollection(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			return _factory.EventTypeCollection;
		}

		public EventType GetEventTypeSingle(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			return _factory.EventTypeSingle;
		}

		public ICollection<EventBean> EvaluateGetROCollectionEvents(
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
		}

		public ICollection<object> EvaluateGetROCollectionScalar(
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
		}

		public EventBean EvaluateGetEventBean(
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
		}

		public CodegenExpression EvaluateGetROCollectionEventsCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return FlexWrap(
				ExprDotMethod(
					future,
					"GetCollectionOfEvents",
					Constant(column),
					exprSymbol.GetAddEPS(parent),
					exprSymbol.GetAddIsNewData(parent),
					exprSymbol.GetAddExprEvalCtx(parent)));
		}

		public CodegenExpression EvaluateGetROCollectionScalarCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return ExprDotMethod(
				future,
				"GetCollectionScalar",
				Constant(column),
				exprSymbol.GetAddEPS(parent),
				exprSymbol.GetAddIsNewData(parent),
				exprSymbol.GetAddExprEvalCtx(parent));
		}

		public CodegenExpression EvaluateGetEventBeanCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return ExprDotMethod(
				future,
				"GetEventBean",
				Constant(column),
				exprSymbol.GetAddEPS(parent),
				exprSymbol.GetAddIsNewData(parent),
				exprSymbol.GetAddExprEvalCtx(parent));
		}

		public ExprEnumerationEval ExprEvaluatorEnumeration => this;

		public AggregationForgeFactory AggregationForgeFactory => _factory;
	}
} // end of namespace

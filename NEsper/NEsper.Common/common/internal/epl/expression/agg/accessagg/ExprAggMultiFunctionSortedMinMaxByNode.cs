///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.access.sorted;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
	public class ExprAggMultiFunctionSortedMinMaxByNode : ExprAggregateNodeBase,
		ExprEnumerationForge,
		ExprAggMultiFunctionNode
	{
		private readonly bool _max;
		private readonly bool _ever;
		private readonly bool _sortedwin;

		private EventType _containedType;
		private AggregationForgeFactory _aggregationForgeFactory;

		public ExprAggMultiFunctionSortedMinMaxByNode(
			bool max,
			bool ever,
			bool sortedwin) : base(false)
		{
			_max = max;
			_ever = ever;
			_sortedwin = sortedwin;
		}

		public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
		{
			AggregationForgeFactoryAccessSorted factory;

			if (validationContext.StatementRawInfo.StatementType == StatementType.CREATE_TABLE) {
				// handle create-table statements (state creator and default accessor, limited to certain options)
				factory = HandleCreateTable(validationContext);
			}
			else if (validationContext.StatementRawInfo.IntoTableName != null) {
				// handle into-table (state provided, accessor and agent needed, validation done by factory)
				factory = HandleIntoTable(validationContext);
			}
			else {
				// handle standalone
				factory = HandleNonTable(validationContext);
			}

			_containedType = factory.ContainedEventType;
			_aggregationForgeFactory = factory;
			return factory;
		}

		private AggregationForgeFactoryAccessSorted HandleNonTable(ExprValidationContext validationContext)
		{
			if (positionalParams.Length == 0) {
				throw new ExprValidationException("Missing the sort criteria expression");
			}

			// validate that the streams referenced in the criteria are a single stream's
			ISet<int> streams = ExprNodeUtilityQuery.GetIdentStreamNumbers(positionalParams[0]);
			if (streams.Count > 1 || streams.IsEmpty()) {
				throw new ExprValidationException(ErrorPrefix + " requires that any parameter expressions evaluate properties of the same stream");
			}

			int streamNum = streams.First();

			// validate that there is a remove stream, use "ever" if not
			if (!_ever && ExprAggMultiFunctionLinearAccessNode.GetIstreamOnly(validationContext.StreamTypeService, streamNum)) {
				if (_sortedwin) {
					throw new ExprValidationException(ErrorPrefix + " requires that a data window is declared for the stream");
				}
			}

			// determine typing and evaluation
			_containedType = validationContext.StreamTypeService.EventTypes[streamNum];

			Type componentType = _containedType.UnderlyingType;
			Type accessorResultType = componentType;
			AggregationAccessorForge accessor;
			TableMetaData tableMetadata = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(_containedType);
			if (!_sortedwin) {
				if (tableMetadata != null) {
					accessor = new AggregationAccessorMinMaxByTable(_max, tableMetadata);
				}
				else {
					accessor = new AggregationAccessorMinMaxByNonTable(_max);
				}
			}
			else {
				if (tableMetadata != null) {
					accessor = new AggregationAccessorSortedTable(_max, componentType, tableMetadata);
				}
				else {
					accessor = new AggregationAccessorSortedNonTable(_max, componentType);
				}

				accessorResultType = TypeHelper.GetArrayType(accessorResultType);
			}

			Pair<ExprNode[], bool[]> criteriaExpressions = CriteriaExpressions;

			AggregationStateTypeWStream type;
			if (_ever) {
				type = _max ? AggregationStateTypeWStream.MAXEVER : AggregationStateTypeWStream.MINEVER;
			}
			else {
				type = AggregationStateTypeWStream.SORTED;
			}

			AggregationStateKeyWStream stateKey = new AggregationStateKeyWStream(streamNum, _containedType, type, criteriaExpressions.First, optionalFilter);

			ExprForge optionalFilterForge = optionalFilter == null ? null : optionalFilter.Forge;
			EventType streamEventType = validationContext.StreamTypeService.EventTypes[streamNum];
			Type[] criteriaTypes = ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions.First);
			DataInputOutputSerdeForge[] criteriaSerdes = new DataInputOutputSerdeForge[criteriaTypes.Length];
			for (int i = 0; i < criteriaTypes.Length; i++) {
				criteriaSerdes[i] = validationContext.SerdeResolver.SerdeForAggregation(criteriaTypes[i], validationContext.StatementRawInfo);
			}

			SortedAggregationStateDesc sortedDesc = new
				SortedAggregationStateDesc(
					_max,
					validationContext.ImportService,
					criteriaExpressions.First,
					criteriaTypes,
					criteriaSerdes,
					criteriaExpressions.Second,
					_ever,
					streamNum,
					this,
					optionalFilterForge,
					streamEventType);

			IList<StmtClassForgeableFactory> serdeForgables = SerdeEventTypeUtility.Plan(
				_containedType,
				validationContext.StatementRawInfo,
				validationContext.SerdeEventTypeRegistry,
				validationContext.SerdeResolver);
			validationContext.AdditionalForgeables.AddAll(serdeForgables);

			return new AggregationForgeFactoryAccessSorted(
				this,
				accessor,
				accessorResultType,
				_containedType,
				stateKey,
				sortedDesc,
				AggregationAgentDefault.INSTANCE);
		}

		public CodegenExpression EvaluateGetROCollectionEventsCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return ExprDotMethod(
				future,
				"getCollectionOfEvents",
				Constant(column),
				exprSymbol.GetAddEPS(parent),
				exprSymbol.GetAddIsNewData(parent),
				exprSymbol.GetAddExprEvalCtx(parent));
		}

		private AggregationForgeFactoryAccessSorted HandleIntoTable(ExprValidationContext validationContext)
		{
			int streamNum;
			if (positionalParams.Length == 0 ||
			    (positionalParams.Length == 1 && positionalParams[0] is ExprWildcard)) {
				ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(validationContext.StreamTypeService, AggregationFunctionName);
				streamNum = 0;
			}
			else if (positionalParams.Length == 1 && positionalParams[0] is ExprStreamUnderlyingNode) {
				streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(positionalParams[0]);
			}
			else if (positionalParams.Length > 0) {
				throw new ExprValidationException("When specifying into-table a sort expression cannot be provided");
			}
			else {
				streamNum = 0;
			}

			EventType containedType = validationContext.StreamTypeService.EventTypes[streamNum];
			Type componentType = containedType.UnderlyingType;
			Type accessorResultType = componentType;
			AggregationAccessorForge accessor;
			if (!_sortedwin) {
				accessor = new AggregationAccessorMinMaxByNonTable(_max);
			}
			else {
				accessor = new AggregationAccessorSortedNonTable(_max, componentType);
				accessorResultType = TypeHelper.GetArrayType(accessorResultType);
			}

			AggregationAgentForge agent = AggregationAgentForgeFactory.Make(
				streamNum,
				optionalFilter,
				validationContext.ImportService,
				validationContext.StreamTypeService.IsOnDemandStreams,
				validationContext.StatementName);
			return new AggregationForgeFactoryAccessSorted(this, accessor, accessorResultType, containedType, null, null, agent);
		}

		private AggregationForgeFactoryAccessSorted HandleCreateTable(ExprValidationContext validationContext)
		{
			if (positionalParams.Length == 0) {
				throw new ExprValidationException("Missing the sort criteria expression");
			}

			string message = "For tables columns, the aggregation function requires the 'sorted(*)' declaration";
			if (!_sortedwin && !_ever) {
				throw new ExprValidationException(message);
			}

			if (validationContext.StreamTypeService.StreamNames.Length == 0) {
				throw new ExprValidationException("'Sorted' requires that the event type is provided");
			}

			EventType containedType = validationContext.StreamTypeService.EventTypes[0];
			Type componentType = containedType.UnderlyingType;
			Pair<ExprNode[], bool[]> criteriaExpressions = CriteriaExpressions;
			Type accessorResultType = componentType;
			AggregationAccessorForge accessor;
			if (!_sortedwin) {
				accessor = new AggregationAccessorMinMaxByNonTable(_max);
			}
			else {
				accessor = new AggregationAccessorSortedNonTable(_max, componentType);
				accessorResultType = TypeHelper.GetArrayType(accessorResultType);
			}

			Type[] criteriaTypes = ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions.First);
			DataInputOutputSerdeForge[] criteriaSerdes = new DataInputOutputSerdeForge[criteriaTypes.Length];
			for (int i = 0; i < criteriaTypes.Length; i++) {
				criteriaSerdes[i] = validationContext.SerdeResolver.SerdeForAggregation(criteriaTypes[i], validationContext.StatementRawInfo);
			}

			SortedAggregationStateDesc stateDesc = new SortedAggregationStateDesc(
				_max,
				validationContext.ImportService,
				criteriaExpressions.First,
				criteriaTypes,
				criteriaSerdes,
				criteriaExpressions.Second,
				_ever,
				0,
				this,
				null,
				containedType);

			IList<StmtClassForgeableFactory> serdeForgables = SerdeEventTypeUtility.Plan(
				containedType,
				validationContext.StatementRawInfo,
				validationContext.SerdeEventTypeRegistry,
				validationContext.SerdeResolver);
			validationContext.AdditionalForgeables.AddAll(serdeForgables);

			return new AggregationForgeFactoryAccessSorted(this, accessor, accessorResultType, containedType, null, stateDesc, null);
		}

		private Pair<ExprNode[], bool[]> CriteriaExpressions {
			get {
				// determine ordering ascending/descending and build criteria expression without "asc" marker
				ExprNode[] criteriaExpressions = new ExprNode[positionalParams.Length];
				bool[] sortDescending = new bool[positionalParams.Length];
				for (int i = 0; i < positionalParams.Length; i++) {
					ExprNode parameter = positionalParams[i];
					criteriaExpressions[i] = parameter;
					if (parameter is ExprOrderedExpr) {
						ExprOrderedExpr ordered = (ExprOrderedExpr) parameter;
						sortDescending[i] = ordered.IsDescending;
						if (!ordered.IsDescending) {
							criteriaExpressions[i] = ordered.ChildNodes[0];
						}
					}
				}

				return new Pair<ExprNode[], bool[]>(criteriaExpressions, sortDescending);
			}
		}

		public override string AggregationFunctionName {
			get {
				if (_sortedwin) {
					return "sorted";
				}

				if (_ever) {
					return _max ? "maxbyever" : "minbyever";
				}

				return _max ? "maxby" : "minby";
			}
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			writer.Write(AggregationFunctionName);
			ExprNodeUtilityPrint.ToExpressionStringParams(writer, positionalParams);
		}

		public CodegenExpression EvaluateGetROCollectionScalarCodegen(
			CodegenMethodScope codegenMethodScope,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			return ConstantNull();
		}

		public EventType GetEventTypeCollection(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			if (!_sortedwin) {
				return null;
			}

			return _containedType;
		}

		public Type ComponentTypeCollection {
			get { return null; }
		}

		public EventType GetEventTypeSingle(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			if (_sortedwin) {
				return null;
			}

			return _containedType;
		}

		public CodegenExpression EvaluateGetEventBeanCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return ExprDotMethod(
				future,
				"getEventBean",
				Constant(column),
				exprSymbol.GetAddEPS(parent),
				exprSymbol.GetAddIsNewData(parent),
				exprSymbol.GetAddExprEvalCtx(parent));
		}

		public bool IsMax {
			get { return _max; }
		}

		public override bool IsFilterExpressionAsLastParameter {
			get { return false; }
		}

		public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
		{
			if (!(node is ExprAggMultiFunctionSortedMinMaxByNode)) {
				return false;
			}

			ExprAggMultiFunctionSortedMinMaxByNode other = (ExprAggMultiFunctionSortedMinMaxByNode) node;
			return _max == other._max && _containedType == other._containedType && _sortedwin == other._sortedwin && _ever == other._ever;
		}

		public ExprEnumerationEval ExprEvaluatorEnumeration {
			get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
		}

		public AggregationForgeFactory AggregationForgeFactory {
			get { return _aggregationForgeFactory; }
		}

		private string ErrorPrefix {
			get { return "The '" + AggregationFunctionName + "' aggregation function"; }
		}
	}
} // end of namespace

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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.access.linear;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.accessagg
{
	public class ExprAggMultiFunctionLinearAccessNode : ExprAggregateNodeBase,
		ExprEnumerationForge,
		ExprAggMultiFunctionNode
	{
		private readonly AggregationAccessorLinearType _stateType;
		private AggregationForgeFactory _aggregationForgeFactory;
		private EventType _containedType;
		private Type _scalarCollectionComponentType;
		private EventType _streamType;

		public ExprAggMultiFunctionLinearAccessNode(AggregationAccessorLinearType stateType) : base(false)
		{
			_stateType = stateType;
		}

		public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
		{
			AggregationLinearFactoryDesc desc;

			// handle table-access expression (state provided, accessor needed)
			if (validationContext.StatementRawInfo.StatementType == StatementType.CREATE_TABLE) {
				// handle create-table statements (state creator and default accessor, limited to certain options)
				desc = HandleCreateTable(positionalParams, _stateType, validationContext);
			}
			else if (validationContext.StatementRawInfo.IntoTableName != null) {
				// handle into-table (state provided, accessor and agent needed, validation done by factory)
				desc = HandleIntoTable(positionalParams, _stateType, validationContext);
			}
			else {
				// handle standalone
				desc = HandleNonIntoTable(positionalParams, _stateType, validationContext);
			}

			_containedType = desc.EnumerationEventType;
			_scalarCollectionComponentType = desc.ScalarCollectionType;

			EventType[] streamTypes = validationContext.StreamTypeService.EventTypes;
			_streamType = desc.StreamNum >= streamTypes.Length ? streamTypes[0] : streamTypes[desc.StreamNum];

			_aggregationForgeFactory = desc.Factory;
			return _aggregationForgeFactory;
		}

		public AggregationForgeFactory AggregationForgeFactory {
			get { return _aggregationForgeFactory; }
		}

		public ExprEnumerationEval ExprEvaluatorEnumeration {
			get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
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

		public CodegenExpression EvaluateGetROCollectionEventsCodegen(
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol exprSymbol,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression future = GetAggFuture(codegenClassScope);
			return ExprDotMethod(
				future,
				"GetCollectionOfEvents",
				Constant(column),
				exprSymbol.GetAddEPS(parent),
				exprSymbol.GetAddIsNewData(parent),
				exprSymbol.GetAddExprEvalCtx(parent));
		}

		private AggregationLinearFactoryDesc HandleNonIntoTable(
			ExprNode[] childNodes,
			AggregationAccessorLinearType stateType,
			ExprValidationContext validationContext)
		{
			StreamTypeService streamTypeService = validationContext.StreamTypeService;
			int streamNum;
			Type resultType;
			ExprForge forge;
			ExprNode evaluatorIndex = null;
			bool istreamOnly;
			EventType containedType;
			Type scalarCollectionComponentType = null;

			// validate wildcard use
			bool isWildcard = childNodes.Length == 0 || childNodes.Length > 0 && childNodes[0] is ExprWildcard;
			if (isWildcard) {
				ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(validationContext.StreamTypeService, stateType.ToString().ToLowerInvariant());
				streamNum = 0;
				containedType = streamTypeService.EventTypes[0];
				resultType = containedType.UnderlyingType;
				TableMetaData tableMetadataX = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(containedType);
				forge = ExprNodeUtilityMake.MakeUnderlyingForge(0, resultType, tableMetadataX);
				istreamOnly = GetIstreamOnly(streamTypeService, 0);
				if ((stateType == AggregationAccessorLinearType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
					throw MakeUnboundValidationEx(stateType);
				}
			}
			else if (childNodes.Length > 0 && childNodes[0] is ExprStreamUnderlyingNode) {
				// validate "stream.*"
				streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(childNodes[0]);
				istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
				if ((stateType == AggregationAccessorLinearType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
					throw MakeUnboundValidationEx(stateType);
				}

				EventType type = streamTypeService.EventTypes[streamNum];
				containedType = type;
				resultType = type.UnderlyingType;
				TableMetaData tableMetadataX = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(type);
				forge = ExprNodeUtilityMake.MakeUnderlyingForge(streamNum, resultType, tableMetadataX);
			}
			else {
				// validate when neither wildcard nor "stream.*"
				ExprNode child = childNodes[0];
				ISet<int> streams = ExprNodeUtilityQuery.GetIdentStreamNumbers(child);
				if (streams.IsEmpty() || (streams.Count > 1)) {
					throw new ExprValidationException(
						GetErrorPrefix(stateType) +
						" requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead");
				}

				streamNum = streams.First();
				istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
				if ((stateType == AggregationAccessorLinearType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
					throw MakeUnboundValidationEx(stateType);
				}

				resultType = childNodes[0].Forge.EvaluationType;
				forge = childNodes[0].Forge;
				if (streamNum >= streamTypeService.EventTypes.Length) {
					containedType = streamTypeService.EventTypes[0];
				}
				else {
					containedType = streamTypeService.EventTypes[streamNum];
				}

				scalarCollectionComponentType = resultType;
			}

			if (childNodes.Length > 1) {
				if (stateType == AggregationAccessorLinearType.WINDOW) {
					throw new ExprValidationException(GetErrorPrefix(stateType) + " does not accept an index expression; Use 'first' or 'last' instead");
				}

				evaluatorIndex = childNodes[1];
				Type indexResultType = evaluatorIndex.Forge.EvaluationType;
				if (indexResultType != typeof(int?) && indexResultType != typeof(int)) {
					throw new ExprValidationException(GetErrorPrefix(stateType) + " requires an index expression that returns an integer value");
				}
			}

			// determine accessor
			AggregationAccessorForge accessor;
			if (evaluatorIndex != null) {
				bool isFirst = stateType == AggregationAccessorLinearType.FIRST;
				int constant = -1;
				ExprForge forgeIndex;
				if (evaluatorIndex.Forge.ForgeConstantType.IsCompileTimeConstant) {
					constant = evaluatorIndex.Forge.ExprEvaluator.Evaluate(null, true, null).AsInt32();
					forgeIndex = null;
				}
				else {
					forgeIndex = evaluatorIndex.Forge;
				}

				accessor = new AggregationAccessorFirstLastIndexWEvalForge(streamNum, forge, forgeIndex, constant, isFirst);
			}
			else {
				if (stateType == AggregationAccessorLinearType.FIRST) {
					accessor = new AggregationAccessorFirstWEvalForge(streamNum, forge);
				}
				else if (stateType == AggregationAccessorLinearType.LAST) {
					accessor = new AggregationAccessorLastWEvalForge(streamNum, forge);
				}
				else if (stateType == AggregationAccessorLinearType.WINDOW) {
					accessor = new AggregationAccessorWindowWEvalForge(streamNum, forge, resultType);
				}
				else {
					throw new IllegalStateException("Access type is undefined or not known as code '" + stateType + "'");
				}
			}

			Type accessorResultType = resultType;
			if (stateType == AggregationAccessorLinearType.WINDOW) {
				accessorResultType = TypeHelper.GetArrayType(resultType);
			}

			bool isFafWindow = streamTypeService.IsOnDemandStreams && stateType == AggregationAccessorLinearType.WINDOW;
			TableMetaData tableMetadata = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(containedType);

			if (tableMetadata == null && !isFafWindow && (istreamOnly || streamTypeService.IsOnDemandStreams)) {
				if (optionalFilter != null) {
					positionalParams = ExprNodeUtilityMake.AddExpression(positionalParams, optionalFilter);
				}

				DataInputOutputSerdeForge serde = validationContext.SerdeResolver.SerdeForAggregation(accessorResultType, validationContext.StatementRawInfo);
				AggregationForgeFactory factoryX = new AggregationForgeFactoryFirstLastUnbound(this, accessorResultType, optionalFilter != null, serde);
				return new AggregationLinearFactoryDesc(factoryX, containedType, scalarCollectionComponentType, streamNum);
			}

			AggregationStateKeyWStream stateKey = new AggregationStateKeyWStream(
				streamNum,
				containedType,
				AggregationStateTypeWStream.DATAWINDOWACCESS_LINEAR,
				ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY,
				optionalFilter);

			ExprForge optionalFilterForge = optionalFilter == null ? null : optionalFilter.Forge;
			AggregationStateFactoryForge stateFactory = new AggregationStateLinearForge(this, streamNum, optionalFilterForge);

			AggregationForgeFactoryAccessLinear factory = new AggregationForgeFactoryAccessLinear(
				this,
				accessor,
				accessorResultType,
				stateKey,
				stateFactory,
				AggregationAgentDefault.INSTANCE,
				containedType);
			EventType enumerationType = scalarCollectionComponentType == null ? containedType : null;

			IList<StmtClassForgeableFactory> serdeForgables = SerdeEventTypeUtility.Plan(
				containedType,
				validationContext.StatementRawInfo,
				validationContext.SerdeEventTypeRegistry,
				validationContext.SerdeResolver);
			validationContext.AdditionalForgeables.AddAll(serdeForgables);

			return new AggregationLinearFactoryDesc(factory, enumerationType, scalarCollectionComponentType, streamNum);
		}

		private AggregationLinearFactoryDesc HandleCreateTable(
			ExprNode[] childNodes,
			AggregationAccessorLinearType stateType,
			ExprValidationContext validationContext)
		{
			string message = "For tables columns, the " +
			                 stateType.GetName().ToLowerInvariant() +
			                 " aggregation function requires the 'window(*)' declaration";
			if (stateType != AggregationAccessorLinearType.WINDOW) {
				throw new ExprValidationException(message);
			}

			if (childNodes.Length == 0 || childNodes.Length > 1 || !(childNodes[0] is ExprWildcard)) {
				throw new ExprValidationException(message);
			}

			if (validationContext.StreamTypeService.StreamNames.Length == 0) {
				throw new ExprValidationException(GetErrorPrefix(stateType) + " requires that the event type is provided");
			}

			EventType containedType = validationContext.StreamTypeService.EventTypes[0];
			Type componentType = containedType.UnderlyingType;
			AggregationAccessorForge accessor = new AggregationAccessorWindowNoEvalForge(componentType);
			AggregationStateFactoryForge stateFactory = new AggregationStateLinearForge(this, 0, null);
			AggregationForgeFactoryAccessLinear factory = new AggregationForgeFactoryAccessLinear(
				this,
				accessor,
				TypeHelper.GetArrayType(componentType),
				null,
				stateFactory,
				null,
				containedType);

			IList<StmtClassForgeableFactory> additionalForgeables = SerdeEventTypeUtility.Plan(
				containedType,
				validationContext.StatementRawInfo,
				validationContext.SerdeEventTypeRegistry,
				validationContext.SerdeResolver);
			validationContext.AdditionalForgeables.AddAll(additionalForgeables);

			return new AggregationLinearFactoryDesc(factory, containedType, null, 0);
		}

		private AggregationLinearFactoryDesc HandleIntoTable(
			ExprNode[] childNodes,
			AggregationAccessorLinearType stateType,
			ExprValidationContext validationContext)
		{
			string message = "For into-table use 'window(*)' or 'window(stream.*)' instead";
			if (stateType != AggregationAccessorLinearType.WINDOW) {
				throw new ExprValidationException(message);
			}

			if (childNodes.Length == 0 || childNodes.Length > 1) {
				throw new ExprValidationException(message);
			}

			if (validationContext.StreamTypeService.StreamNames.Length == 0) {
				throw new ExprValidationException(GetErrorPrefix(stateType) + " requires that at least one stream is provided");
			}

			int streamNum;
			if (childNodes[0] is ExprWildcard) {
				if (validationContext.StreamTypeService.StreamNames.Length != 1) {
					throw new ExprValidationException(GetErrorPrefix(stateType) + " with wildcard requires a single stream");
				}

				streamNum = 0;
			}
			else if (childNodes[0] is ExprStreamUnderlyingNode) {
				ExprStreamUnderlyingNode und = (ExprStreamUnderlyingNode) childNodes[0];
				streamNum = und.StreamId;
			}
			else {
				throw new ExprValidationException(message);
			}

			EventType containedType = validationContext.StreamTypeService.EventTypes[streamNum];
			Type componentType = containedType.UnderlyingType;
			AggregationAccessorForge accessor = new AggregationAccessorWindowNoEvalForge(componentType);
			AggregationAgentForge agent = AggregationAgentForgeFactory.Make(
				streamNum,
				optionalFilter,
				validationContext.ImportService,
				validationContext.StreamTypeService.IsOnDemandStreams,
				validationContext.StatementName);
			AggregationForgeFactoryAccessLinear factory = new AggregationForgeFactoryAccessLinear(
				this,
				accessor,
				TypeHelper.GetArrayType(componentType),
				null,
				null,
				agent,
				containedType);
			return new AggregationLinearFactoryDesc(factory, containedType, null, 0);
		}

		public static bool GetIstreamOnly(
			StreamTypeService streamTypeService,
			int streamNum)
		{
			if (streamNum < streamTypeService.EventTypes.Length) {
				return streamTypeService.IStreamOnly[streamNum];
			}

			// this could happen for match-recognize which has different stream types for selection and for aggregation
			return streamTypeService.IStreamOnly[0];
		}

		public override string AggregationFunctionName {
			get { return _stateType.ToString().ToLowerInvariant(); }
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			writer.Write(_stateType.ToString().ToLowerInvariant());
			ExprNodeUtilityPrint.ToExpressionStringParams(writer, this.ChildNodes);
		}

		public AggregationAccessorLinearType StateType {
			get { return _stateType; }
		}

		public EventType GetEventTypeCollection(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			if (_stateType == AggregationAccessorLinearType.FIRST || _stateType == AggregationAccessorLinearType.LAST) {
				return null;
			}

			return _containedType;
		}

		public Type ComponentTypeCollection {
			get { return _scalarCollectionComponentType; }
		}

		public EventType GetEventTypeSingle(
			StatementRawInfo statementRawInfo,
			StatementCompileTimeServices compileTimeServices)
		{
			if (_stateType == AggregationAccessorLinearType.FIRST || _stateType == AggregationAccessorLinearType.LAST) {
				return _containedType;
			}

			return null;
		}

		public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
		{
			if (!(node is ExprAggMultiFunctionLinearAccessNode)) {
				return false;
			}

			ExprAggMultiFunctionLinearAccessNode other = (ExprAggMultiFunctionLinearAccessNode) node;
			return _stateType == other._stateType && _containedType == other._containedType && _scalarCollectionComponentType == other._scalarCollectionComponentType;
		}

		private static ExprValidationException MakeUnboundValidationEx(AggregationAccessorLinearType stateType)
		{
			return new ExprValidationException(
				GetErrorPrefix(stateType) +
				" requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead");
		}

		private static string GetErrorPrefix(AggregationAccessorLinearType stateType)
		{
			return ExprAggMultiFunctionUtil.GetErrorPrefix(stateType.ToString().ToLowerInvariant());
		}

		public override bool IsFilterExpressionAsLastParameter {
			get { return false; }
		}

		public EventType StreamType {
			get { return _streamType; }
		}
	}
} // end of namespace

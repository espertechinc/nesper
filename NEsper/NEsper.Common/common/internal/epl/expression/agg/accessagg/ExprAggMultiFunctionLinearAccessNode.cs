///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private EventType containedType;

        public ExprAggMultiFunctionLinearAccessNode(AggregationAccessorLinearType stateType)
            : base(false)
        {
            StateType = stateType;
        }

        public ExprNodeRenderable EnumForgeRenderable => ForgeRenderableLocal;

        public override string AggregationFunctionName => StateType.ToString().ToLowerInvariant();

        public AggregationAccessorLinearType StateType { get; }

        public override bool IsFilterExpressionAsLastParameter => false;

        public EventType StreamType { get; private set; }

        public AggregationTableReadDesc ValidateAggregationTableRead(
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccessColumn,
            TableMetaData table)
        {
            var validation = tableAccessColumn.AggregationPortableValidation;
            if (!(validation is AggregationPortableValidationLinear)) {
                throw new ExprValidationException(
                    "Invalid aggregation column type for column '" + tableAccessColumn.ColumnName + "'");
            }

            var validationLinear = (AggregationPortableValidationLinear) validation;

            if (StateType == AggregationAccessorLinearType.FIRST || StateType == AggregationAccessorLinearType.LAST) {
                return HandleTableAccessFirstLast(ChildNodes, StateType, validationContext, validationLinear);
            }

            if (StateType == AggregationAccessorLinearType.WINDOW) {
                return HandleTableAccessWindow(ChildNodes, validationContext, validationLinear);
            }

            throw new IllegalStateException("Unrecognized type " + StateType);
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
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
            var future = GetAggFuture(codegenClassScope);
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
            var future = GetAggFuture(codegenClassScope);
            return FlexWrap(
                ExprDotMethod(
                    future,
                    "GetCollectionOfEvents",
                    Constant(column),
                    exprSymbol.GetAddEPS(parent),
                    exprSymbol.GetAddIsNewData(parent),
                    exprSymbol.GetAddExprEvalCtx(parent)));
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (StateType == AggregationAccessorLinearType.FIRST || StateType == AggregationAccessorLinearType.LAST) {
                return null;
            }

            return containedType;
        }

        public Type ComponentTypeCollection { get; private set; }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (StateType == AggregationAccessorLinearType.FIRST || StateType == AggregationAccessorLinearType.LAST) {
                return containedType;
            }

            return null;
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            AggregationLinearFactoryDesc desc;

            // handle table-access expression (state provided, accessor needed)
            if (validationContext.StatementRawInfo.StatementType == StatementType.CREATE_TABLE) {
                // handle create-table statements (state creator and default accessor, limited to certain options)
                desc = HandleCreateTable(positionalParams, StateType, validationContext);
            }
            else if (validationContext.StatementRawInfo.IntoTableName != null) {
                // handle into-table (state provided, accessor and agent needed, validation done by factory)
                desc = HandleIntoTable(positionalParams, StateType, validationContext);
            }
            else {
                // handle standalone
                desc = HandleNonIntoTable(positionalParams, StateType, validationContext);
            }

            containedType = desc.EnumerationEventType;
            ComponentTypeCollection = desc.ScalarCollectionType;

            var streamTypes = validationContext.StreamTypeService.EventTypes;
            StreamType = desc.StreamNum >= streamTypes.Length ? streamTypes[0] : streamTypes[desc.StreamNum];

            return desc.Factory;
        }

        private AggregationLinearFactoryDesc HandleNonIntoTable(
            ExprNode[] childNodes,
            AggregationAccessorLinearType stateType,
            ExprValidationContext validationContext)
        {
            var streamTypeService = validationContext.StreamTypeService;
            int streamNum;
            Type resultType;
            ExprForge forge;
            ExprNode evaluatorIndex = null;
            bool istreamOnly;
            EventType containedType;
            Type scalarCollectionComponentType = null;

            // validate wildcard use
            var isWildcard = childNodes.Length == 0 || childNodes.Length > 0 && childNodes[0] is ExprWildcard;
            if (isWildcard) {
                ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(
                    validationContext.StreamTypeService,
                    stateType.ToString().ToLowerInvariant());
                streamNum = 0;
                containedType = streamTypeService.EventTypes[0];
                resultType = containedType.UnderlyingType;
                var tableMetadataX =
                    validationContext.TableCompileTimeResolver.ResolveTableFromEventType(containedType);
                forge = ExprNodeUtilityMake.MakeUnderlyingForge(0, resultType, tableMetadataX);
                istreamOnly = GetIstreamOnly(streamTypeService, 0);
                if (stateType == AggregationAccessorLinearType.WINDOW &&
                    istreamOnly &&
                    !streamTypeService.IsOnDemandStreams) {
                    throw MakeUnboundValidationEx(stateType);
                }
            }
            else if (childNodes.Length > 0 && childNodes[0] is ExprStreamUnderlyingNode) {
                // validate "stream.*"
                streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(childNodes[0]);
                istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
                if (stateType == AggregationAccessorLinearType.WINDOW &&
                    istreamOnly &&
                    !streamTypeService.IsOnDemandStreams) {
                    throw MakeUnboundValidationEx(stateType);
                }

                var type = streamTypeService.EventTypes[streamNum];
                containedType = type;
                resultType = type.UnderlyingType;
                var tableMetadataX = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(type);
                forge = ExprNodeUtilityMake.MakeUnderlyingForge(streamNum, resultType, tableMetadataX);
            }
            else {
                // validate when neither wildcard nor "stream.*"
                var child = childNodes[0];
                var streams = ExprNodeUtilityQuery.GetIdentStreamNumbers(child);
                if (streams.IsEmpty() || streams.Count > 1) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) +
                        " requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead");
                }

                streamNum = streams.First();
                istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
                if (stateType == AggregationAccessorLinearType.WINDOW &&
                    istreamOnly &&
                    !streamTypeService.IsOnDemandStreams) {
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
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) +
                        " does not accept an index expression; Use 'first' or 'last' instead");
                }

                evaluatorIndex = childNodes[1];
                var indexResultType = evaluatorIndex.Forge.EvaluationType;
                if (indexResultType != typeof(int?) && indexResultType != typeof(int)) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) + " requires an index expression that returns an integer value");
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

                accessor = new AggregationAccessorFirstLastIndexWEvalForge(
                    streamNum,
                    forge,
                    forgeIndex,
                    constant,
                    isFirst);
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
                    throw new IllegalStateException(
                        "Access type is undefined or not known as code '" + stateType + "'");
                }
            }

            var accessorResultType = resultType;
            if (stateType == AggregationAccessorLinearType.WINDOW) {
                accessorResultType = TypeHelper.GetArrayType(resultType);
            }

            var isFafWindow = streamTypeService.IsOnDemandStreams && stateType == AggregationAccessorLinearType.WINDOW;
            var tableMetadata = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(containedType);

            if (tableMetadata == null && !isFafWindow && (istreamOnly || streamTypeService.IsOnDemandStreams)) {
                if (optionalFilter != null) {
                    positionalParams = ExprNodeUtilityMake.AddExpression(positionalParams, optionalFilter);
                }

                AggregationForgeFactory factory = new AggregationFactoryMethodFirstLastUnbound(
                    this,
                    containedType,
                    accessorResultType,
                    streamNum,
                    optionalFilter != null);
                return new AggregationLinearFactoryDesc(
                    factory,
                    containedType,
                    scalarCollectionComponentType,
                    streamNum);
            }

            var stateKey = new AggregationStateKeyWStream(
                streamNum,
                containedType,
                AggregationStateTypeWStream.DATAWINDOWACCESS_LINEAR,
                ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY,
                optionalFilter);

            var optionalFilterForge = optionalFilter == null ? null : optionalFilter.Forge;
            AggregationStateFactoryForge stateFactory = new AggregationStateLinearForge(
                this,
                streamNum,
                optionalFilterForge);

            var factoryX = new AggregationForgeFactoryAccessLinear(
                this,
                accessor,
                accessorResultType,
                stateKey,
                stateFactory,
                AggregationAgentDefault.INSTANCE,
                containedType);
            var enumerationType = scalarCollectionComponentType == null ? containedType : null;
            return new AggregationLinearFactoryDesc(
                factoryX,
                enumerationType,
                scalarCollectionComponentType,
                streamNum);
        }

        private AggregationLinearFactoryDesc HandleCreateTable(
            ExprNode[] childNodes,
            AggregationAccessorLinearType stateType,
            ExprValidationContext validationContext)
        {
            var message = "For tables columns, the " +
                          stateType.GetName().ToLowerInvariant() +
                          " aggregation function requires the 'window(*)' declaration";
            if (stateType != AggregationAccessorLinearType.WINDOW) {
                throw new ExprValidationException(message);
            }

            if (childNodes.Length == 0 || childNodes.Length > 1 || !(childNodes[0] is ExprWildcard)) {
                throw new ExprValidationException(message);
            }

            if (validationContext.StreamTypeService.StreamNames.Length == 0) {
                throw new ExprValidationException(
                    GetErrorPrefix(stateType) + " requires that the event type is provided");
            }

            var containedType = validationContext.StreamTypeService.EventTypes[0];
            var componentType = containedType.UnderlyingType;
            AggregationAccessorForge accessor = new AggregationAccessorWindowNoEvalForge(componentType);
            AggregationStateFactoryForge stateFactory = new AggregationStateLinearForge(this, 0, null);
            var factory = new AggregationForgeFactoryAccessLinear(
                this,
                accessor,
                TypeHelper.GetArrayType(componentType),
                null,
                stateFactory,
                null,
                containedType);
            return new AggregationLinearFactoryDesc(factory, containedType, null, 0);
        }

        private AggregationLinearFactoryDesc HandleIntoTable(
            ExprNode[] childNodes,
            AggregationAccessorLinearType stateType,
            ExprValidationContext validationContext)
        {
            var message = "For into-table use 'window(*)' or 'window(stream.*)' instead";
            if (stateType != AggregationAccessorLinearType.WINDOW) {
                throw new ExprValidationException(message);
            }

            if (childNodes.Length == 0 || childNodes.Length > 1) {
                throw new ExprValidationException(message);
            }

            if (validationContext.StreamTypeService.StreamNames.Length == 0) {
                throw new ExprValidationException(
                    GetErrorPrefix(stateType) + " requires that at least one stream is provided");
            }

            int streamNum;
            if (childNodes[0] is ExprWildcard) {
                if (validationContext.StreamTypeService.StreamNames.Length != 1) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) + " with wildcard requires a single stream");
                }

                streamNum = 0;
            }
            else if (childNodes[0] is ExprStreamUnderlyingNode) {
                var und = (ExprStreamUnderlyingNode) childNodes[0];
                streamNum = und.StreamId;
            }
            else {
                throw new ExprValidationException(message);
            }

            var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
            var componentType = containedType.UnderlyingType;
            AggregationAccessorForge accessor = new AggregationAccessorWindowNoEvalForge(componentType);
            var agent = AggregationAgentForgeFactory.Make(
                streamNum,
                optionalFilter,
                validationContext.ImportService,
                validationContext.StreamTypeService.IsOnDemandStreams,
                validationContext.StatementName);
            var factory = new AggregationForgeFactoryAccessLinear(
                this,
                accessor,
                TypeHelper.GetArrayType(componentType),
                null,
                null,
                agent,
                containedType);
            return new AggregationLinearFactoryDesc(factory, containedType, null, 0);
        }

        private AggregationTableReadDesc HandleTableAccessFirstLast(
            ExprNode[] childNodes,
            AggregationAccessorLinearType stateType,
            ExprValidationContext validationContext,
            AggregationPortableValidationLinear validationLinear)
        {
            Type underlyingType = validationLinear.ContainedEventType.UnderlyingType;
            if (childNodes.Length == 0) {
                var forge = new AggregationTAAReaderLinearFirstLastForge(underlyingType, stateType, null);
                return new AggregationTableReadDesc(forge, null, null, validationLinear.ContainedEventType);
            }

            if (childNodes.Length == 1) {
                if (childNodes[0] is ExprWildcard) {
                    var forgeX = new AggregationTAAReaderLinearFirstLastForge(underlyingType, stateType, null);
                    return new AggregationTableReadDesc(forgeX, null, null, validationLinear.ContainedEventType);
                }

                if (childNodes[0] is ExprStreamUnderlyingNode) {
                    throw new ExprValidationException("Stream-wildcard is not allowed for table column access");
                }

                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableCompileTimeUtil.StreamTypeFromTableColumn(validationLinear.ContainedEventType);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.AGGPARAM,
                    paramNode,
                    localValidationContext);
                var forge = new AggregationTAAReaderLinearFirstLastForge(
                    paramNode.Forge.EvaluationType,
                    stateType,
                    paramNode);
                return new AggregationTableReadDesc(forge, null, null, null);
            }

            if (childNodes.Length == 2) {
                int? constant = null;
                var indexEvalNode = childNodes[1];
                var indexEvalType = indexEvalNode.Forge.EvaluationType;
                if (indexEvalType != typeof(int?) && indexEvalType != typeof(int)) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) +
                        " requires a constant index expression that returns an integer value");
                }

                ExprNode indexExpr;
                if (indexEvalNode.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    constant = (int?) indexEvalNode.Forge.ExprEvaluator.Evaluate(null, true, null);
                    indexExpr = null;
                }
                else {
                    indexExpr = indexEvalNode;
                }

                var forge = new AggregationTAAReaderLinearFirstLastIndexForge(
                    underlyingType,
                    stateType,
                    constant,
                    indexExpr);
                return new AggregationTableReadDesc(forge, null, null, validationLinear.ContainedEventType);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        private AggregationTableReadDesc HandleTableAccessWindow(
            ExprNode[] childNodes,
            ExprValidationContext validationContext,
            AggregationPortableValidationLinear validationLinear)
        {
            if (childNodes.Length == 0 || childNodes.Length == 1 && childNodes[0] is ExprWildcard) {
                Type componentType = validationLinear.ContainedEventType.UnderlyingType;
                var forge = new AggregationTAAReaderLinearWindowForge(TypeHelper.GetArrayType(componentType), null);
                return new AggregationTableReadDesc(forge, validationLinear.ContainedEventType, null, null);
            }

            if (childNodes.Length == 1) {
                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableCompileTimeUtil.StreamTypeFromTableColumn(validationLinear.ContainedEventType);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.AGGPARAM,
                    paramNode,
                    localValidationContext);
                var paramNodeType = paramNode.Forge.EvaluationType.GetBoxedType();
                var forge = new AggregationTAAReaderLinearWindowForge(
                    TypeHelper.GetArrayType(paramNodeType),
                    paramNode);
                return new AggregationTableReadDesc(forge, null, paramNodeType, null);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        protected internal static bool GetIstreamOnly(
            StreamTypeService streamTypeService,
            int streamNum)
        {
            if (streamNum < streamTypeService.EventTypes.Length) {
                return streamTypeService.IStreamOnly[streamNum];
            }

            // this could happen for match-recognize which has different stream types for selection and for aggregation
            return streamTypeService.IStreamOnly[0];
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(StateType.ToString().ToLowerInvariant());
            ExprNodeUtilityPrint.ToExpressionStringParams(writer, ChildNodes);
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprAggMultiFunctionLinearAccessNode)) {
                return false;
            }

            var other = (ExprAggMultiFunctionLinearAccessNode) node;
            return StateType == other.StateType &&
                   containedType == other.containedType &&
                   ComponentTypeCollection == other.ComponentTypeCollection;
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
    }
} // end of namespace
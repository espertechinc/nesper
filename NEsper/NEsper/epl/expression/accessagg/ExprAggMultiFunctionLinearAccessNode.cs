///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    [Serializable]
    public class ExprAggMultiFunctionLinearAccessNode : ExprAggregateNodeBase
        , ExprEvaluatorEnumeration
        , ExprAggregateAccessMultiValueNode
    {
        [NonSerialized] private EventType _containedType;

        [NonSerialized] private Type _scalarCollectionComponentType;

        public ExprAggMultiFunctionLinearAccessNode(AggregationStateType stateType)
            : base(false)
        {
            StateType = stateType;
        }

        public override string AggregationFunctionName => StateType.ToString().ToLowerInvariant();

        public AggregationStateType StateType { get; }

        protected override bool IsFilterExpressionAsLastParameter => false;

        public AggregationMethodFactory ValidateAggregationParamsWBinding(
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccessColumn)
        {
            return ValidateAggregationInternal(validationContext, tableAccessColumn);
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            if (StateType == AggregationStateType.FIRST || StateType == AggregationStateType.LAST) return null;
            return _containedType;
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            if (StateType == AggregationStateType.FIRST || StateType == AggregationStateType.LAST)
                return _containedType;
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetCollectionOfEvents(Column, evaluateParams);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetCollectionScalar(Column, evaluateParams);
        }

        public Type ComponentTypeCollection => _scalarCollectionComponentType;

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetEventBean(Column, evaluateParams);
        }

        protected override AggregationMethodFactory ValidateAggregationChild(
            ExprValidationContext validationContext)
        {
            return ValidateAggregationInternal(validationContext, null);
        }

        private AggregationMethodFactory ValidateAggregationInternal(
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation optionalBinding)
        {
            LinearAggregationFactoryDesc desc;

            // handle table-access expression (state provided, accessor needed)
            var positionalParams = PositionalParams;
            if (optionalBinding != null)
                desc = HandleTableAccess(positionalParams, StateType, validationContext, optionalBinding);
            else if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE)
                desc = HandleCreateTable(positionalParams, StateType, validationContext);
            else if (validationContext.IntoTableName != null)
                desc = HandleIntoTable(positionalParams, StateType, validationContext);
            else
                desc = HandleNonIntoTable(positionalParams, StateType, validationContext);

            _containedType = desc.EnumerationEventType;
            _scalarCollectionComponentType = desc.ScalarCollectionType;

            return desc.Factory;
        }

        private LinearAggregationFactoryDesc HandleNonIntoTable(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext)
        {
            var streamTypeService = validationContext.StreamTypeService;
            int streamNum;
            Type resultType;
            ExprEvaluator evaluator;
            ExprNode evaluatorIndex = null;
            bool istreamOnly;
            EventType containedType;
            Type scalarCollectionComponentType = null;

            // validate wildcard use
            var isWildcard = childNodes.Length == 0 || childNodes.Length > 0 && childNodes[0] is ExprWildcard;
            if (isWildcard)
            {
                ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(
                    validationContext.StreamTypeService, stateType.ToString().ToLowerInvariant());
                streamNum = 0;
                containedType = streamTypeService.EventTypes[0];
                resultType = containedType.UnderlyingType;
                var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(containedType);
                evaluator = ExprNodeUtility.MakeUnderlyingEvaluator(0, resultType, tableMetadata);
                istreamOnly = GetIstreamOnly(streamTypeService, 0);
                if (stateType == AggregationStateType.WINDOW && istreamOnly && !streamTypeService.IsOnDemandStreams)
                    throw MakeUnboundValidationEx(stateType);
            }
            else if (childNodes.Length > 0 && childNodes[0] is ExprStreamUnderlyingNode)
            {
                // validate "stream.*"
                streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(childNodes[0]);
                istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
                if (stateType == AggregationStateType.WINDOW && istreamOnly && !streamTypeService.IsOnDemandStreams)
                    throw MakeUnboundValidationEx(stateType);
                var type = streamTypeService.EventTypes[streamNum];
                containedType = type;
                resultType = type.UnderlyingType;
                var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(type);
                evaluator = ExprNodeUtility.MakeUnderlyingEvaluator(streamNum, resultType, tableMetadata);
            }
            else
            {
                // validate when neither wildcard nor "stream.*"
                var child = childNodes[0];
                var streams = ExprNodeUtility.GetIdentStreamNumbers(child);
                if (streams.IsEmpty() || streams.Count > 1)
                    throw new ExprValidationException(GetErrorPrefix(stateType) +
                                                      " requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead");
                streamNum = streams.First();
                istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
                if (stateType == AggregationStateType.WINDOW && istreamOnly && !streamTypeService.IsOnDemandStreams)
                    throw MakeUnboundValidationEx(stateType);
                resultType = childNodes[0].ExprEvaluator.ReturnType;
                evaluator = childNodes[0].ExprEvaluator;
                if (streamNum >= streamTypeService.EventTypes.Length)
                    containedType = streamTypeService.EventTypes[0];
                else
                    containedType = streamTypeService.EventTypes[streamNum];
                scalarCollectionComponentType = resultType;
            }

            if (childNodes.Length > 1)
            {
                if (stateType == AggregationStateType.WINDOW)
                    throw new ExprValidationException(GetErrorPrefix(stateType) +
                                                      " does not accept an index expression; Use 'first' or 'last' instead");
                evaluatorIndex = childNodes[1];
                if (!evaluatorIndex.ExprEvaluator.ReturnType.IsInt32()) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) +
                        " requires an index expression that returns an integer value");
                }
            }

            // determine accessor
            AggregationAccessor accessor;
            if (evaluatorIndex != null)
            {
                var isFirst = stateType == AggregationStateType.FIRST;
                var constant = -1;
                if (evaluatorIndex.IsConstantResult)
                    constant = evaluatorIndex.ExprEvaluator.Evaluate(EvaluateParams.EmptyTrue).AsInt();
                accessor = new AggregationAccessorFirstLastIndexWEval(streamNum, evaluator,
                    evaluatorIndex.ExprEvaluator, constant, isFirst);
            }
            else
            {
                if (stateType == AggregationStateType.FIRST)
                    accessor = new AggregationAccessorFirstWEval(streamNum, evaluator);
                else if (stateType == AggregationStateType.LAST)
                    accessor = new AggregationAccessorLastWEval(streamNum, evaluator);
                else if (stateType == AggregationStateType.WINDOW)
                    accessor = new AggregationAccessorWindowWEval(streamNum, evaluator, resultType);
                else
                    throw new IllegalStateException("Access type is undefined or not known as code '" + stateType +
                                                    "'");
            }

            var accessorResultType = resultType;
            if (stateType == AggregationStateType.WINDOW) accessorResultType = TypeHelper.GetArrayType(resultType);

            var isFafWindow = streamTypeService.IsOnDemandStreams && stateType == AggregationStateType.WINDOW;
            var tableMetadataX = validationContext.TableService.GetTableMetadataFromEventType(containedType);

            var optionalFilter = OptionalFilter;

            if (tableMetadataX == null && !isFafWindow && (istreamOnly || streamTypeService.IsOnDemandStreams))
            {
                if (optionalFilter != null)
                    PositionalParams = ExprNodeUtility.AddExpression(PositionalParams, optionalFilter);
                var factory = validationContext.EngineImportService.AggregationFactoryFactory.MakeLinearUnbounded(
                    validationContext.StatementExtensionSvcContext, this, containedType, accessorResultType, streamNum,
                    optionalFilter != null);
                return new LinearAggregationFactoryDesc(factory, containedType, scalarCollectionComponentType);
            }

            var stateKey = new AggregationStateKeyWStream(
                streamNum, containedType,
                AggregationStateTypeWStream.DATAWINDOWACCESS_LINEAR, 
                new ExprNode[0], optionalFilter);

            var optionalFilterEval = optionalFilter?.ExprEvaluator;
            var stateFactory = validationContext.EngineImportService.AggregationFactoryFactory.MakeLinear(
                validationContext.StatementExtensionSvcContext, this, streamNum, optionalFilterEval);
            var factoryX = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, accessorResultType,
                containedType,
                stateKey, stateFactory, AggregationAgentDefault.INSTANCE);
            var enumerationType = scalarCollectionComponentType == null ? containedType : null;
            return new LinearAggregationFactoryDesc(factoryX, enumerationType, scalarCollectionComponentType);
        }

        private LinearAggregationFactoryDesc HandleCreateTable(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext)
        {
            var message = "For tables columns, the " + stateType.GetName().ToLowerInvariant() +
                          " aggregation function requires the 'window(*)' declaration";
            if (stateType != AggregationStateType.WINDOW) throw new ExprValidationException(message);
            if (childNodes.Length == 0 || childNodes.Length > 1 || !(childNodes[0] is ExprWildcard))
                throw new ExprValidationException(message);
            if (validationContext.StreamTypeService.StreamNames.Length == 0)
                throw new ExprValidationException(GetErrorPrefix(stateType) +
                                                  " requires that the event type is provided");
            var containedType = validationContext.StreamTypeService.EventTypes[0];
            var componentType = containedType.UnderlyingType;
            var accessor = new AggregationAccessorWindowNoEval(componentType);
            var stateFactory = validationContext.EngineImportService.AggregationFactoryFactory.MakeLinear(
                validationContext.StatementExtensionSvcContext, this, 0, null);
            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor,
                TypeHelper.GetArrayType(componentType), containedType, null, stateFactory, null);
            return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
        }

        private LinearAggregationFactoryDesc HandleIntoTable(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext)
        {
            var message = "For into-table use 'window(*)' or ''window(stream.*)' instead";
            if (stateType != AggregationStateType.WINDOW) throw new ExprValidationException(message);
            if (childNodes.Length == 0 || childNodes.Length > 1) throw new ExprValidationException(message);
            if (validationContext.StreamTypeService.StreamNames.Length == 0)
                throw new ExprValidationException(GetErrorPrefix(stateType) +
                                                  " requires that at least one stream is provided");
            int streamNum;
            if (childNodes[0] is ExprWildcard)
            {
                if (validationContext.StreamTypeService.StreamNames.Length != 1)
                    throw new ExprValidationException(GetErrorPrefix(stateType) +
                                                      " with wildcard requires a single stream");
                streamNum = 0;
            }
            else if (childNodes[0] is ExprStreamUnderlyingNode)
            {
                var und = (ExprStreamUnderlyingNode) childNodes[0];
                streamNum = und.StreamId;
            }
            else
            {
                throw new ExprValidationException(message);
            }

            var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
            var componentType = containedType.UnderlyingType;
            var accessor = new AggregationAccessorWindowNoEval(componentType);
            var agent = ExprAggAggregationAgentFactory.Make(streamNum, OptionalFilter);
            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor,
                TypeHelper.GetArrayType(componentType), containedType, null, null, agent);
            return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
        }

        private LinearAggregationFactoryDesc HandleTableAccess(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccess)
        {
            if (stateType == AggregationStateType.FIRST || stateType == AggregationStateType.LAST)
                return HandleTableAccessFirstLast(childNodes, stateType, validationContext, tableAccess);
            if (stateType == AggregationStateType.WINDOW)
                return HandleTableAccessWindow(childNodes, stateType, validationContext, tableAccess);
            throw new IllegalStateException("Unrecognized type " + stateType);
        }

        private LinearAggregationFactoryDesc HandleTableAccessFirstLast(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccess)
        {
            var original = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) tableAccess.Factory;
            var resultType = original.ContainedEventType.UnderlyingType;
            var defaultAccessor = stateType == AggregationStateType.FIRST
                ? AggregationAccessorFirstNoEval.INSTANCE
                : (AggregationAccessor) AggregationAccessorLastNoEval.INSTANCE;
            if (childNodes.Length == 0)
            {
                var factoryAccess = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, defaultAccessor,
                    resultType, original.ContainedEventType, null, null, null);
                return new LinearAggregationFactoryDesc(factoryAccess, factoryAccess.ContainedEventType, null);
            }

            if (childNodes.Length == 1)
            {
                if (childNodes[0] is ExprWildcard)
                {
                    var factoryAccess = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, defaultAccessor,
                        resultType, original.ContainedEventType, null, null, null);
                    return new LinearAggregationFactoryDesc(factoryAccess, factoryAccess.ContainedEventType, null);
                }

                if (childNodes[0] is ExprStreamUnderlyingNode)
                    throw new ExprValidationException("Stream-wildcard is not allowed for table column access");
                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableServiceUtil.StreamTypeFromTableColumn(tableAccess,
                    validationContext.StreamTypeService.EngineURIQualifier);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode,
                    localValidationContext);
                var paramNodeEval = paramNode.ExprEvaluator;
                AggregationAccessor accessor;
                if (stateType == AggregationStateType.FIRST)
                    accessor = new AggregationAccessorFirstWEval(0, paramNodeEval);
                else
                    accessor = new AggregationAccessorLastWEval(0, paramNodeEval);
                var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(
                    this, accessor, paramNodeEval.ReturnType, original.ContainedEventType, null, null, null);
                return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
            }

            if (childNodes.Length == 2)
            {
                var isFirst = stateType == AggregationStateType.FIRST;
                var constant = -1;
                var indexEvalNode = childNodes[1];
                if (indexEvalNode.IsConstantResult)
                    constant = indexEvalNode.ExprEvaluator.Evaluate(EvaluateParams.EmptyTrue).AsInt();
                var evaluatorIndex = indexEvalNode.ExprEvaluator;
                if (evaluatorIndex.ReturnType.IsInt32()) {
                    throw new ExprValidationException(
                        GetErrorPrefix(stateType) +
                        " requires a constant index expression that returns an integer value");
                }

                var accessor = new AggregationAccessorFirstLastIndexNoEval(evaluatorIndex, constant, isFirst);
                var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, resultType,
                    original.ContainedEventType, null, null, null);
                return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        private LinearAggregationFactoryDesc HandleTableAccessWindow(
            ExprNode[] childNodes,
            AggregationStateType stateType,
            ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccess)
        {
            var original = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) tableAccess.Factory;
            if (childNodes.Length == 0 ||
                childNodes.Length == 1 && childNodes[0] is ExprWildcard)
            {
                var componentType = original.ContainedEventType.UnderlyingType;
                var accessor = new AggregationAccessorWindowNoEval(componentType);
                var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor,
                    TypeHelper.GetArrayType(componentType), original.ContainedEventType, null, null, null);
                return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
            }

            if (childNodes.Length == 1)
            {
                // Expressions apply to events held, thereby validate in terms of event value expressions
                var paramNode = childNodes[0];
                var streams = TableServiceUtil.StreamTypeFromTableColumn(tableAccess,
                    validationContext.StreamTypeService.EngineURIQualifier);
                var localValidationContext = new ExprValidationContext(streams, validationContext);
                paramNode = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode,
                    localValidationContext);
                var paramNodeEval = paramNode.ExprEvaluator;
                var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this,
                    new AggregationAccessorWindowWEval(0, paramNodeEval, paramNodeEval.ReturnType),
                    TypeHelper.GetArrayType(paramNodeEval.ReturnType), original.ContainedEventType, null, null, null);
                return new LinearAggregationFactoryDesc(factory, null, paramNodeEval.ReturnType);
            }

            throw new ExprValidationException("Invalid number of parameters");
        }

        internal static bool GetIstreamOnly(StreamTypeService streamTypeService, int streamNum)
        {
            if (streamNum < streamTypeService.EventTypes.Length) return streamTypeService.IsIStreamOnly[streamNum];
            // this could happen for match-recognize which has different stream types for selection and for aggregation
            return streamTypeService.IsIStreamOnly[0];
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(StateType.ToString().ToLowerInvariant());
            ExprNodeUtility.ToExpressionStringParams(writer, ChildNodes);
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

        private static ExprValidationException MakeUnboundValidationEx(AggregationStateType stateType)
        {
            return new ExprValidationException(GetErrorPrefix(stateType) +
                                               " requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead");
        }

        private static string GetErrorPrefix(AggregationStateType stateType)
        {
            return ExprAggMultiFunctionUtil.GetErrorPrefix(stateType.ToString().ToLowerInvariant());
        }
    }
} // end of namespace
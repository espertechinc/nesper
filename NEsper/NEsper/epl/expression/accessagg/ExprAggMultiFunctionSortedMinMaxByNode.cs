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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    [Serializable]
    public class ExprAggMultiFunctionSortedMinMaxByNode : ExprAggregateNodeBase
        , ExprEvaluatorEnumeration
        , ExprAggregateAccessMultiValueNode
    {
        private readonly bool _ever;
        private readonly bool _sortedwin;

        [NonSerialized] private EventType _containedType;

        public ExprAggMultiFunctionSortedMinMaxByNode(bool max, bool ever, bool sortedwin)
            : base(false)
        {
            IsMax = max;
            this._ever = ever;
            this._sortedwin = sortedwin;
        }

        private Pair<ExprNode[], bool[]> CriteriaExpressions
        {
            get
            {
                // determine ordering ascending/descending and build criteria expression without "asc" marker
                var positionalParams = PositionalParams;
                var criteriaExpressions = new ExprNode[positionalParams.Length];
                var sortDescending = new bool[positionalParams.Length];
                for (var i = 0; i < positionalParams.Length; i++)
                {
                    var parameter = positionalParams[i];
                    criteriaExpressions[i] = parameter;
                    if (parameter is ExprOrderedExpr)
                    {
                        var ordered = (ExprOrderedExpr) parameter;
                        sortDescending[i] = ordered.IsDescending;
                        if (!ordered.IsDescending) criteriaExpressions[i] = ordered.ChildNodes[0];
                    }
                }

                return new Pair<ExprNode[], bool[]>(criteriaExpressions, sortDescending);
            }
        }

        public override string AggregationFunctionName
        {
            get
            {
                if (_sortedwin) return "sorted";

                if (_ever) return IsMax ? "maxbyever" : "minbyever";

                return IsMax ? "maxby" : "minby";
            }
        }

        public bool IsMax { get; }

        protected override bool IsFilterExpressionAsLastParameter => false;

        public AggregationMethodFactory ValidateAggregationParamsWBinding(ExprValidationContext validationContext,
            TableMetadataColumnAggregation tableAccessColumn)
        {
            return ValidateAggregationInternal(validationContext, tableAccessColumn);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetCollectionOfEvents(Column, evaluateParams);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return null;
        }

        public EventType GetEventTypeCollection(
            EventAdapterService eventAdapterService,
            int statementId)
        {
            if (!_sortedwin) return null;
            return _containedType;
        }

        public Type ComponentTypeCollection => null;

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            if (_sortedwin) return null;
            return _containedType;
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            return AggregationResultFuture.GetEventBean(Column, evaluateParams);
        }

        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            return ValidateAggregationInternal(validationContext, null);
        }

        private AggregationMethodFactory ValidateAggregationInternal(ExprValidationContext validationContext,
            TableMetadataColumnAggregation optionalBinding)
        {
            ExprAggMultiFunctionSortedMinMaxByNodeFactory factory;

            // handle table-access expression (state provided, accessor needed)
            if (optionalBinding != null)
                factory = HandleTableAccess(optionalBinding);
            else if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE)
                factory = HandleCreateTable(validationContext);
            else if (validationContext.IntoTableName != null)
                factory = HandleIntoTable(validationContext);
            else
                factory = HandleNonTable(validationContext);

            _containedType = factory.ContainedEventType;
            return factory;
        }

        private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleNonTable(ExprValidationContext validationContext)
        {
            var positionalParams = PositionalParams;
            if (positionalParams.Length == 0)
                throw new ExprValidationException("Missing the sort criteria expression");

            // validate that the streams referenced in the criteria are a single stream's
            var streams = ExprNodeUtility.GetIdentStreamNumbers(positionalParams[0]);
            if (streams.Count > 1 || streams.IsEmpty())
                throw new ExprValidationException(
                    GetErrorPrefix() + " requires that any parameter expressions evaluate properties of the same stream");
            var streamNum = streams.First();

            // validate that there is a remove stream, use "ever" if not
            if (!_ever && ExprAggMultiFunctionLinearAccessNode.GetIstreamOnly(validationContext.StreamTypeService,
                    streamNum))
                if (_sortedwin)
                    throw new ExprValidationException(
                        GetErrorPrefix() + " requires that a data window is declared for the stream");

            // determine typing and evaluation
            _containedType = validationContext.StreamTypeService.EventTypes[streamNum];

            var componentType = _containedType.UnderlyingType;
            var accessorResultType = componentType;
            AggregationAccessor accessor;
            var tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(_containedType);
            if (!_sortedwin)
            {
                if (tableMetadata != null)
                    accessor = new AggregationAccessorMinMaxByTable(IsMax, tableMetadata);
                else
                    accessor = new AggregationAccessorMinMaxByNonTable(IsMax);
            }
            else
            {
                if (tableMetadata != null)
                    accessor = new AggregationAccessorSortedTable(IsMax, componentType, tableMetadata);
                else
                    accessor = new AggregationAccessorSortedNonTable(IsMax, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var criteriaExpressions = CriteriaExpressions;

            AggregationStateTypeWStream type;
            if (_ever)
                type = IsMax ? AggregationStateTypeWStream.MAXEVER : AggregationStateTypeWStream.MINEVER;
            else
                type = AggregationStateTypeWStream.SORTED;

            var optionalFilter = OptionalFilter;
            var stateKey = new AggregationStateKeyWStream(
                streamNum, _containedType, type, criteriaExpressions.First, optionalFilter);

            var optionalFilterEval = optionalFilter?.ExprEvaluator;
            var stateFactoryFactory = new
                SortedAggregationStateFactoryFactory(validationContext.EngineImportService,
                    validationContext.StatementExtensionSvcContext,
                    ExprNodeUtility.GetEvaluators(criteriaExpressions.First),
                    criteriaExpressions.Second, _ever, streamNum, this, optionalFilterEval);

            return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, _containedType,
                stateKey, stateFactoryFactory, AggregationAgentDefault.INSTANCE);
        }

        private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleIntoTable(
            ExprValidationContext validationContext)
        {
            int streamNum;
            var positionalParams = PositionalParams;
            if (positionalParams.Length == 0 ||
                positionalParams.Length == 1 && positionalParams[0] is ExprWildcard)
            {
                ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(validationContext.StreamTypeService,
                    AggregationFunctionName);
                streamNum = 0;
            }
            else if (positionalParams.Length == 1 && positionalParams[0] is ExprStreamUnderlyingNode)
            {
                streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(positionalParams[0]);
            }
            else if (positionalParams.Length > 0)
            {
                throw new ExprValidationException("When specifying into-table a sort expression cannot be provided");
            }
            else
            {
                streamNum = 0;
            }

            var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
            var componentType = containedType.UnderlyingType;
            var accessorResultType = componentType;
            AggregationAccessor accessor;
            if (!_sortedwin)
            {
                accessor = new AggregationAccessorMinMaxByNonTable(IsMax);
            }
            else
            {
                accessor = new AggregationAccessorSortedNonTable(IsMax, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var agent = ExprAggAggregationAgentFactory.Make(streamNum, OptionalFilter);
            return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, containedType,
                null, null, agent);
        }

        private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleCreateTable(
            ExprValidationContext validationContext)
        {
            if (PositionalParams.Length == 0)
                throw new ExprValidationException("Missing the sort criteria expression");

            var message = "For tables columns, the aggregation function requires the 'sorted(*)' declaration";
            if (!_sortedwin && !_ever) throw new ExprValidationException(message);
            if (validationContext.StreamTypeService.StreamNames.Length == 0)
                throw new ExprValidationException("'Sorted' requires that the event type is provided");
            var containedType = validationContext.StreamTypeService.EventTypes[0];
            var componentType = containedType.UnderlyingType;
            var criteriaExpressions = CriteriaExpressions;
            var accessorResultType = componentType;
            AggregationAccessor accessor;
            if (!_sortedwin)
            {
                accessor = new AggregationAccessorMinMaxByNonTable(IsMax);
            }
            else
            {
                accessor = new AggregationAccessorSortedNonTable(IsMax, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            var stateFactoryFactory = new
                SortedAggregationStateFactoryFactory(validationContext.EngineImportService,
                    validationContext.StatementExtensionSvcContext,
                    ExprNodeUtility.GetEvaluators(criteriaExpressions.First),
                    criteriaExpressions.Second, _ever, 0, this, null);
            return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, containedType,
                null, stateFactoryFactory, null);
        }

        private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleTableAccess(
            TableMetadataColumnAggregation tableAccess)
        {
            var factory = (ExprAggMultiFunctionSortedMinMaxByNodeFactory) tableAccess.Factory;
            AggregationAccessor accessor;
            var componentType = factory.ContainedEventType.UnderlyingType;
            var accessorResultType = componentType;
            if (!_sortedwin)
            {
                accessor = new AggregationAccessorMinMaxByNonTable(IsMax);
            }
            else
            {
                accessor = new AggregationAccessorSortedNonTable(IsMax, componentType);
                accessorResultType = TypeHelper.GetArrayType(accessorResultType);
            }

            return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType,
                factory.ContainedEventType, null, null, null);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(AggregationFunctionName);
            ExprNodeUtility.ToExpressionStringParams(writer, PositionalParams);
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return false;
        }

        private string GetErrorPrefix()
        {
            return "The '" + AggregationFunctionName + "' aggregation function";
        }
    }
} // end of namespace
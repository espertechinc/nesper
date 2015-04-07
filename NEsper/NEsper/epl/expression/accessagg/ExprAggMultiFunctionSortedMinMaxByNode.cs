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

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
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
	public class ExprAggMultiFunctionSortedMinMaxByNode 
        : ExprAggregateNodeBase
        , ExprEvaluatorEnumeration
        , ExprAggregateAccessMultiValueNode
	{
	    private readonly bool _max;
	    private readonly bool _ever;
	    private readonly bool _sortedwin;

        [NonSerialized]
	    private EventType _containedType;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public ExprAggMultiFunctionSortedMinMaxByNode(bool max, bool ever, bool sortedwin)
	        : base(false)
        {
	        _max = max;
	        _ever = ever;
	        _sortedwin = sortedwin;
	    }

	    public AggregationMethodFactory ValidateAggregationParamsWBinding(ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccessColumn)
        {
	        return ValidateAggregationInternal(validationContext, tableAccessColumn);
	    }

	    public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
	        return ValidateAggregationInternal(validationContext, null);
	    }

	    private AggregationMethodFactory ValidateAggregationInternal(ExprValidationContext validationContext, TableMetadataColumnAggregation optionalBinding)
	    {
	        ExprAggMultiFunctionSortedMinMaxByNodeFactory factory;

	        // handle table-access expression (state provided, accessor needed)
	        if (optionalBinding != null) {
	            factory = HandleTableAccess(optionalBinding);
	        }
	        // handle create-table statements (state creator and default accessor, limited to certain options)
	        else if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE) {
	            factory = HandleCreateTable(validationContext);
	        }
	        // handle into-table (state provided, accessor and agent needed, validation done by factory)
	        else if (validationContext.IntoTableName != null) {
	            factory = HandleIntoTable(validationContext);
	        }
	        // handle standalone
	        else {
	            factory = HandleNonTable(validationContext);
	        }

	        _containedType = factory.ContainedEventType;
	        return factory;
	    }

	    private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleNonTable(ExprValidationContext validationContext)

	    {
	        if (PositionalParams.Length == 0) {
	            throw new ExprValidationException("Missing the sort criteria expression");
	        }

	        // validate that the streams referenced in the criteria are a single stream's
            var streams = ExprNodeUtility.GetIdentStreamNumbers(PositionalParams[0]);
	        if (streams.Count > 1 || streams.IsEmpty()) {
	            throw new ExprValidationException(ErrorPrefix + " requires that any parameter expressions evaluate properties of the same stream");
	        }
	        int streamNum = streams.First();

	        // validate that there is a remove stream, use "ever" if not
	        var forceEver = false;
	        if (!_ever && ExprAggMultiFunctionLinearAccessNode.GetIstreamOnly(validationContext.StreamTypeService, streamNum)) {
	            if (_sortedwin) {
	                throw new ExprValidationException(ErrorPrefix + " requires that a data window is declared for the stream");
	            }
	            forceEver = true;
	        }

	        // determine typing and evaluation
	        _containedType = validationContext.StreamTypeService.EventTypes[streamNum];

	        var componentType = _containedType.UnderlyingType;
	        var accessorResultType = componentType;
	        AggregationAccessor accessor;
	        TableMetadata tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(_containedType);
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
	        var stateKey = new AggregationStateKeyWStream(streamNum, _containedType, type, criteriaExpressions.First);

	        var stateFactoryFactory = new
	                SortedAggregationStateFactoryFactory(validationContext.MethodResolutionService,
	                ExprNodeUtility.GetEvaluators(criteriaExpressions.First),
	                criteriaExpressions.Second, _ever, streamNum, this);

	        return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, _containedType, stateKey, stateFactoryFactory, AggregationAgentDefault.INSTANCE);
	    }

	    private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleIntoTable(ExprValidationContext validationContext)

	    {
	        int streamNum;
            if (PositionalParams.Length == 0 ||
               (PositionalParams.Length == 1 && PositionalParams[0] is ExprWildcard))
            {
	            ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(validationContext.StreamTypeService, AggregationFunctionName);
	            streamNum = 0;
	        }
            else if (PositionalParams.Length == 1 && PositionalParams[0] is ExprStreamUnderlyingNode)
            {
                streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(PositionalParams[0]);
	        }
            else if (PositionalParams.Length > 0)
            {
	            throw new ExprValidationException("When specifying into-table a sort expression cannot be provided");
	        }
	        else {
	            streamNum = 0;
	        }

	        var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
	        var componentType = containedType.UnderlyingType;
	        var accessorResultType = componentType;
	        AggregationAccessor accessor;
	        if (!_sortedwin) {
	            accessor = new AggregationAccessorMinMaxByNonTable(_max);
	        }
	        else {
	            accessor = new AggregationAccessorSortedNonTable(_max, componentType);
	            accessorResultType = TypeHelper.GetArrayType(accessorResultType);
	        }

	        AggregationAgent agent = AggregationAgentDefault.INSTANCE;
	        if (streamNum != 0) {
	            agent = new AggregationAgentRewriteStream(streamNum);
	        }

	        return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, containedType, null, null, agent);
	    }

	    private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleCreateTable(ExprValidationContext validationContext)
	    {
            if (PositionalParams.Length == 0)
            {
	            throw new ExprValidationException("Missing the sort criteria expression");
	        }

	        const string message = "For tables columns, the aggregation function requires the 'sorted(*)' declaration";

	        if (!_sortedwin && !_ever) {
	            throw new ExprValidationException(message);
	        }
	        if (validationContext.StreamTypeService.StreamNames.Length == 0) {
	            throw new ExprValidationException("'Sorted' requires that the event type is provided");
	        }
	        var containedType = validationContext.StreamTypeService.EventTypes[0];
	        var componentType = containedType.UnderlyingType;
	        Pair<ExprNode[], bool[]> criteriaExpressions = CriteriaExpressions;
	        var accessorResultType = componentType;
	        AggregationAccessor accessor;
	        if (!_sortedwin) {
	            accessor = new AggregationAccessorMinMaxByNonTable(_max);
	        }
	        else {
	            accessor = new AggregationAccessorSortedNonTable(_max, componentType);
	            accessorResultType = TypeHelper.GetArrayType(accessorResultType);
	        }
	        var stateFactoryFactory = new
	                SortedAggregationStateFactoryFactory(validationContext.MethodResolutionService,
	                    ExprNodeUtility.GetEvaluators(criteriaExpressions.First),
	                    criteriaExpressions.Second, _ever, 0, this);
	        return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, containedType, null, stateFactoryFactory, null);
	    }

        private Pair<ExprNode[], bool[]> CriteriaExpressions
        {
            get
            {
                // determine ordering ascending/descending and build criteria expression without "asc" marker
                var criteriaExpressions = new ExprNode[PositionalParams.Length];
                var sortDescending = new bool[PositionalParams.Length];
                for (var i = 0; i < PositionalParams.Length; i++)
                {
                    var parameter = PositionalParams[i];
                    criteriaExpressions[i] = parameter;
                    if (parameter is ExprOrderedExpr)
                    {
                        var ordered = (ExprOrderedExpr) parameter;
                        sortDescending[i] = ordered.IsDescending;
                        if (!ordered.IsDescending)
                        {
                            criteriaExpressions[i] = ordered.ChildNodes[0];
                        }
                    }
                }
                return new Pair<ExprNode[], bool[]>(criteriaExpressions, sortDescending);
            }
        }

        private ExprAggMultiFunctionSortedMinMaxByNodeFactory HandleTableAccess(TableMetadataColumnAggregation tableAccess)
        {
	        var factory = (ExprAggMultiFunctionSortedMinMaxByNodeFactory) tableAccess.Factory;
	        AggregationAccessor accessor;
	        var componentType = factory.ContainedEventType.UnderlyingType;
	        var accessorResultType = componentType;
	        if (!_sortedwin) {
	            accessor = new AggregationAccessorMinMaxByNonTable(_max);
	        }
	        else {
	            accessor = new AggregationAccessorSortedNonTable(_max, componentType);
	            accessorResultType = TypeHelper.GetArrayType(accessorResultType);
	        }
	        return new ExprAggMultiFunctionSortedMinMaxByNodeFactory(this, accessor, accessorResultType, factory.ContainedEventType, null, null, null);
	    }

        public override string AggregationFunctionName
        {
            get
            {
                if (_sortedwin)
                {
                    return "sorted";
                }
                if (_ever)
                {
                    return _max ? "maxbyever" : "minbyever";
                }
                return _max ? "maxby" : "minby";
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write(AggregationFunctionName);
            ExprNodeUtility.ToExpressionStringParams(writer, PositionalParams);
	    }

	    public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return base.AggregationResultFuture.GetCollectionOfEvents(Column, eventsPerStream, isNewData, context);
	    }

	    public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return null;
	    }

	    public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, string statementId) {
	        if (!_sortedwin) {
	            return null;
	        }
	        return _containedType;
	    }

        public Type ComponentTypeCollection
        {
            get { return null; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, string statementId) {
	        if (_sortedwin) {
	            return null;
	        }
	        return _containedType;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return base.AggregationResultFuture.GetEventBean(Column, eventsPerStream, isNewData, context);
	    }

        public bool IsMax
        {
            get { return _max; }
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node) {
	        return false;
	    }

        private string ErrorPrefix
        {
            get { return "The '" + AggregationFunctionName + "' aggregation function"; }
        }
	}
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.accessagg
{
	public class ExprAggMultiFunctionSortedMinMaxByNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprAggMultiFunctionSortedMinMaxByNode _parent;
	    private readonly AggregationAccessor _accessor;
	    private readonly Type _accessorResultType;
	    private readonly EventType _containedEventType;

	    private readonly AggregationStateKey _optionalStateKey;
	    private readonly SortedAggregationStateFactoryFactory _optionalStateFactory;
	    private readonly AggregationAgent _optionalAgent;

	    public ExprAggMultiFunctionSortedMinMaxByNodeFactory(ExprAggMultiFunctionSortedMinMaxByNode parent, AggregationAccessor accessor, Type accessorResultType, EventType containedEventType, AggregationStateKey optionalStateKey, SortedAggregationStateFactoryFactory optionalStateFactory, AggregationAgent optionalAgent)
        {
	        _parent = parent;
	        _accessor = accessor;
	        _accessorResultType = accessorResultType;
	        _containedEventType = containedEventType;
	        _optionalStateKey = optionalStateKey;
	        _optionalStateFactory = optionalStateFactory;
	        _optionalAgent = optionalAgent;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return true; }
	    }

	    public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
	        throw new UnsupportedOperationException();
	    }

	    public Type ResultType
	    {
	        get { return _accessorResultType; }
	    }

	    public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
	        return _optionalStateKey;
	    }

	    public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
	        if (isMatchRecognize || _optionalStateFactory == null)
            {
	            return null;
	        }
	        return _optionalStateFactory.MakeFactory();
	    }

	    public AggregationAccessor Accessor
	    {
	        get { return _accessor; }
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        var other = (ExprAggMultiFunctionSortedMinMaxByNodeFactory) intoTableAgg;
	        AggregationMethodFactoryUtil.ValidateEventType(_containedEventType, other.ContainedEventType);
	        AggregationMethodFactoryUtil.ValidateAggFuncName(_parent.AggregationFunctionName, other.Parent.AggregationFunctionName);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return _optionalAgent; }
	    }

	    public EventType ContainedEventType
	    {
	        get { return _containedEventType; }
	    }

	    public ExprAggMultiFunctionSortedMinMaxByNode Parent
	    {
	        get { return _parent; }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return null;
        }
	}
} // end of namespace

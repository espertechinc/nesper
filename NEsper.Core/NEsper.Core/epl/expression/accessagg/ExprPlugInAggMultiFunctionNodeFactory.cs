///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.plugin;

using AggregationMethodFactoryUtil = com.espertech.esper.epl.agg.service.AggregationMethodFactoryUtil;

namespace com.espertech.esper.epl.expression.accessagg
{
	public class ExprPlugInAggMultiFunctionNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprPlugInAggMultiFunctionNode _parent;
	    private readonly PlugInAggregationMultiFunctionHandler _handlerPlugin;
        private readonly AggregationFactoryFactory _aggregationFactoryFactory;
        private readonly StatementExtensionSvcContext _statementExtensionSvcContext;

	    private EPType _returnType;

        public ExprPlugInAggMultiFunctionNodeFactory(ExprPlugInAggMultiFunctionNode parent, PlugInAggregationMultiFunctionHandler handlerPlugin, AggregationFactoryFactory aggregationFactoryFactory, StatementExtensionSvcContext statementExtensionSvcContext)
        {
            _handlerPlugin = handlerPlugin;
            _parent = parent;
            _aggregationFactoryFactory = aggregationFactoryFactory;
            _statementExtensionSvcContext = statementExtensionSvcContext;
        }

	    public bool IsAccessAggregation
	    {
	        get { return true; }
	    }

	    public AggregationMethod Make()
        {
	        return null;
	    }

	    public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
	        return _handlerPlugin.AggregationStateUniqueKey;
	    }

	    public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            return _aggregationFactoryFactory.MakePlugInAccess(_statementExtensionSvcContext, this);
	    }

	    public AggregationAccessor Accessor
	    {
	        get { return _handlerPlugin.Accessor; }
	    }

	    public Type ResultType
	    {
	        get
	        {
	            ObtainReturnType();
	            return _returnType.GetNormalizedClass();
	        }
	    }

	    public PlugInAggregationMultiFunctionHandler HandlerPlugin
	    {
	        get { return _handlerPlugin; }
	    }

	    public Type ComponentTypeCollection
	    {
	        get
	        {
	            ObtainReturnType();
	            return EPTypeHelper.GetClassMultiValued(_returnType);
	        }
	    }

	    public EventType EventTypeSingle
	    {
	        get
	        {
	            ObtainReturnType();
	            return EPTypeHelper.GetEventTypeSingleValued(_returnType);
	        }
	    }

	    public EventType EventTypeCollection
	    {
	        get
	        {
	            ObtainReturnType();
	            return EPTypeHelper.GetEventTypeMultiValued(_returnType);
	        }
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    private void ObtainReturnType()
        {
	        if (_returnType == null) {
	            _returnType = _handlerPlugin.ReturnType;
	        }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        var that = (ExprPlugInAggMultiFunctionNodeFactory) intoTableAgg;
	        if (!GetAggregationStateKey(false).Equals(that.GetAggregationStateKey(false))) {
	            throw new ExprValidationException("Mismatched state key");
	        }
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get
	        {
	            var ctx = new PlugInAggregationMultiFunctionAgentContext(_parent.ChildNodes);
	            return _handlerPlugin.GetAggregationAgent(ctx);
	        }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }
	}
} // end of namespace

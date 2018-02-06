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

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprPlugInAggMultiFunctionNodeFactory : AggregationMethodFactory
    {
        private readonly AggregationFactoryFactory _aggregationFactoryFactory;
        private readonly ExprPlugInAggMultiFunctionNode _parent;
        private readonly StatementExtensionSvcContext _statementExtensionSvcContext;
        private EPType _returnType;

        public ExprPlugInAggMultiFunctionNodeFactory(
            ExprPlugInAggMultiFunctionNode parent,
            PlugInAggregationMultiFunctionHandler handlerPlugin,
            AggregationFactoryFactory aggregationFactoryFactory,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            HandlerPlugin = handlerPlugin;
            _parent = parent;
            _aggregationFactoryFactory = aggregationFactoryFactory;
            _statementExtensionSvcContext = statementExtensionSvcContext;
        }

        public PlugInAggregationMultiFunctionHandler HandlerPlugin { get; }

        public Type ComponentTypeCollection
        {
            get
            {
                ObtainReturnType();
                return _returnType.GetClassMultiValued();
            }
        }

        public EventType EventTypeSingle
        {
            get
            {
                ObtainReturnType();
                return _returnType.GetEventTypeSingleValued();
            }
        }

        public EventType EventTypeCollection
        {
            get
            {
                ObtainReturnType();
                return _returnType.GetEventTypeMultiValued();
            }
        }

        public AggregationMethod Make()
        {
            return null;
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return HandlerPlugin.AggregationStateUniqueKey;
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            return _aggregationFactoryFactory.MakePlugInAccess(_statementExtensionSvcContext, this);
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (ExprPlugInAggMultiFunctionNodeFactory) intoTableAgg;
            if (!GetAggregationStateKey(false).Equals(that.GetAggregationStateKey(false)))
                throw new ExprValidationException("Mismatched state key");
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }

        public bool IsAccessAggregation => true;

        public AggregationAccessor Accessor => HandlerPlugin.Accessor;

        public Type ResultType
        {
            get
            {
                ObtainReturnType();
                return _returnType.GetNormalizedClass();
            }
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public AggregationAgent AggregationStateAgent
        {
            get
            {
                var ctx = new PlugInAggregationMultiFunctionAgentContext(
                    _parent.ChildNodes, _parent.OptionalFilter);
                return HandlerPlugin.GetAggregationAgent(ctx);
            }
        }

        private void ObtainReturnType()
        {
            if (_returnType == null) _returnType = HandlerPlugin.ReturnType;
        }
    }
} // end of namespace
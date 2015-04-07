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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
    public class ExprCountEverNodeFactory : AggregationMethodFactory
    {
        private readonly ExprCountEverNode _parent;
        private readonly bool _ignoreNulls;
    
        public ExprCountEverNodeFactory(ExprCountEverNode parent, bool ignoreNulls)
        {
            _parent = parent;
            _ignoreNulls = ignoreNulls;
        }

        public bool IsAccessAggregation
        {
            get { return false; }
        }

        public Type ResultType
        {
            get { return typeof (long); }
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }
    
        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationAccessor Accessor
        {
            get { throw new IllegalStateException("Not an access aggregation function"); }
        }

        public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
            return methodResolutionService.MakeCountEverValueAggregator(agentInstanceId, groupId, aggregationId, _parent.HasFilter, _ignoreNulls);
        }

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
            ExprCountEverNodeFactory that = (ExprCountEverNodeFactory) intoTableAgg;
            if (that._ignoreNulls != _ignoreNulls) {
                throw new ExprValidationException("The aggregation declares " +
                        (_ignoreNulls ? "ignore-nulls" : "no-ignore-nulls") +
                        " and provided is " +
                        (that._ignoreNulls ? "ignore-nulls" : "no-ignore-nulls"));
            }
            AggregationMethodFactoryUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
        }

        public AggregationAgent AggregationStateAgent
        {
            get { return null; }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
}

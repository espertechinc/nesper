///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationStateFactoryLinear : AggregationStateFactory
    {
        private readonly ExprAggMultiFunctionLinearAccessNode _expr;
        private readonly ExprEvaluator _optionalFilter;
        private readonly int _streamNum;

        public AggregationStateFactoryLinear(ExprAggMultiFunctionLinearAccessNode expr, int streamNum,
            ExprEvaluator optionalFilter)
        {
            _expr = expr;
            _streamNum = streamNum;
            _optionalFilter = optionalFilter;
        }

        public AggregationState CreateAccess(
            int agentInstanceId,
            bool join,
            object groupKey,
            AggregationServicePassThru passThru)
        {
            if (join)
            {
                return _optionalFilter != null 
                    ? new AggregationStateLinearJoinWFilter(_streamNum, _optionalFilter) 
                    : new AggregationStateLinearJoinImpl(_streamNum);
            }

            return _optionalFilter != null 
                ? new AggregationStateLinearWFilter(_streamNum, _optionalFilter) 
                : new AggregationStateLinearImpl(_streamNum);
        }

        public ExprNode AggregationExpression => _expr;
    }
} // end of namespace
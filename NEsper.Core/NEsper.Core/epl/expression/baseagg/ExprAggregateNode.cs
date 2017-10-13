///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.baseagg
{
    /// <summary>
    /// Base expression node that represents an aggregation function such as 'sum' or 'count'.
    /// </summary>
    public interface ExprAggregateNode : ExprEvaluator, ExprNode
    {
        AggregationMethodFactory Factory { get; }
        void SetAggregationResultFuture(AggregationResultFuture aggregationResultFuture, int column);
        bool IsDistinct { get; }
        ExprAggregateLocalGroupByDesc OptionalLocalGroupBy { get;  }
        void ValidatePositionals();
        ExprNode[] PositionalParams { get; }
    }
}

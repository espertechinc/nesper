///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.@base
{
    /// <summary>
    ///     Base expression node that represents an aggregation function such as 'sum' or 'count'.
    /// </summary>
    public interface ExprAggregateNode : ExprForge,
        ExprNode
    {
        AggregationForgeFactory Factory { get; }

        ExprAggregateLocalGroupByDesc OptionalLocalGroupBy { get; }

        ExprNode[] PositionalParams { get; }

        ExprNode OptionalFilter { get; }

        int Column { get; set; }

        bool IsDistinct { get; }

        void ValidatePositionals(ExprValidationContext validationContext);
    }
} // end of namespace
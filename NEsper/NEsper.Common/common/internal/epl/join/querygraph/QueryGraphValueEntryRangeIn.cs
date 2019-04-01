///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryRangeIn : QueryGraphValueEntryRange
    {
        public QueryGraphValueEntryRangeIn(
            QueryGraphRangeEnum rangeType, ExprEvaluator exprStart, ExprEvaluator exprEnd,
            bool allowRangeReversal) : base(rangeType)
        {
            if (!rangeType.IsRange) {
                throw new ArgumentException("Range type expected but received " + rangeType.GetName());
            }

            ExprStart = exprStart;
            ExprEnd = exprEnd;
            IsAllowRangeReversal = allowRangeReversal;
        }

        public bool IsAllowRangeReversal { get; }

        public ExprEvaluator ExprStart { get; }

        public ExprEvaluator ExprEnd { get; }

        public override ExprEvaluator[] Expressions => new[] {ExprStart, ExprEnd};
    }
} // end of namespace
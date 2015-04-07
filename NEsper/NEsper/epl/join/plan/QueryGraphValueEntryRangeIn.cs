///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValueEntryRangeIn : QueryGraphValueEntryRange
    {
        public QueryGraphValueEntryRangeIn(QueryGraphRangeEnum rangeType, ExprNode exprStart, ExprNode exprEnd, bool allowRangeReversal)
            : base(rangeType)
        {
            if (!rangeType.IsRange()) {
                throw new ArgumentException("Range type expected but received " + rangeType.GetName());
            }
            ExprStart = exprStart;
            ExprEnd = exprEnd;
            IsAllowRangeReversal = allowRangeReversal;
        }

        public bool IsAllowRangeReversal { get; private set; }

        public ExprNode ExprStart { get; private set; }

        public ExprNode ExprEnd { get; private set; }

        public override String ToQueryPlan()
        {
            return RangeType.GetName();
        }

        public override ExprNode[] Expressions
        {
            get
            {
                return new ExprNode[]
                {
                    ExprStart,
                    ExprEnd
                };
            }
        }
    }
}

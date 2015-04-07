///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValueEntryRangeRelOp : QueryGraphValueEntryRange
    {
        public QueryGraphValueEntryRangeRelOp(QueryGraphRangeEnum type, ExprNode expression, bool isBetweenPart)
            : base(type)
        {
            if (type.IsRange()) {
                throw new ArgumentException("Invalid ctor for use with ranges");
            }
            Expression = expression;
            IsBetweenPart = isBetweenPart;
        }

        public ExprNode Expression { get; private set; }

        public bool IsBetweenPart { get; private set; }

        public override String ToQueryPlan() {
            return RangeType.GetStringOp() + " on " + Expression.ToExpressionStringMinPrecedenceSafe();
        }

        public override ExprNode[] Expressions
        {
            get
            {
                return new ExprNode[] { Expression };
            }
        }
    }
}

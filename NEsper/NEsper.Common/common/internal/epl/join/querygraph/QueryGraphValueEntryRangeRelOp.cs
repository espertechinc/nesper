///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryRangeRelOp : QueryGraphValueEntryRange
    {
        public QueryGraphValueEntryRangeRelOp(
            QueryGraphRangeEnum type,
            ExprEvaluator expression,
            bool isBetweenPart)
            : base(type)
        {
            if (type.IsRange) {
                throw new ArgumentException("Invalid ctor for use with ranges");
            }

            Expression = expression;
            IsBetweenPart = isBetweenPart;
        }

        public ExprEvaluator Expression { get; }

        public bool IsBetweenPart { get; }

        public override ExprEvaluator[] Expressions => new[] {Expression};
    }
} // end of namespace
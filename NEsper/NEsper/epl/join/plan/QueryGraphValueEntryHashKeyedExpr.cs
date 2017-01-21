///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class QueryGraphValueEntryHashKeyedExpr : QueryGraphValueEntryHashKeyed
    {
        public QueryGraphValueEntryHashKeyedExpr(ExprNode keyExpr, bool requiresKey)
            : base(keyExpr)
        {
            IsRequiresKey = requiresKey;
        }

        public bool IsRequiresKey { get; private set; }

        public bool IsConstant
        {
            get { return ExprNodeUtility.IsConstantValueExpr(KeyExpr); }
        }

        public override String ToQueryPlan()
        {
            return KeyExpr.ToExpressionStringMinPrecedenceSafe();
        }
    }
}

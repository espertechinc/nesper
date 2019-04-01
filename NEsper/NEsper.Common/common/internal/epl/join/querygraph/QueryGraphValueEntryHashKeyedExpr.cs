///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryHashKeyedExpr : QueryGraphValueEntryHashKeyed
    {
        public QueryGraphValueEntryHashKeyedExpr(ExprEvaluator keyExpr, bool requiresKey)
            : base(keyExpr)
        {
            IsRequiresKey = requiresKey;
        }

        public bool IsRequiresKey { get; }
    }
} // end of namespace
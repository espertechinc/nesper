///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValueEntryHashKeyedForgeProp : QueryGraphValueEntryHashKeyedForgeExpr
    {
        public QueryGraphValueEntryHashKeyedForgeProp(
            ExprNode keyExpr,
            string keyProperty,
            EventPropertyGetterSPI eventPropertyGetter)
            : base(keyExpr, true)
        {
            KeyProperty = keyProperty;
            EventPropertyGetter = eventPropertyGetter;
        }

        public string KeyProperty { get; }

        public EventPropertyGetterSPI EventPropertyGetter { get; }

        public string ToQueryPlan()
        {
            return KeyProperty;
        }
    }
} // end of namespace
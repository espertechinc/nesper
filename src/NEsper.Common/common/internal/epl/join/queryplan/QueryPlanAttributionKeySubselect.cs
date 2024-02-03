///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class QueryPlanAttributionKeySubselect : QueryPlanAttributionKey
    {
        private readonly int subqueryNum;

        public QueryPlanAttributionKeySubselect(int subqueryNum)
        {
            this.subqueryNum = subqueryNum;
        }

        public T Accept<T>(QueryPlanAttributionKeyVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public int SubqueryNum => subqueryNum;
    }
} // end of namespace
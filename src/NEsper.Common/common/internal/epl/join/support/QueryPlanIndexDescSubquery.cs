///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public class QueryPlanIndexDescSubquery : QueryPlanIndexDescBase
    {
        private readonly int subqueryNum;
        private readonly string tableLookupStrategy;

        public QueryPlanIndexDescSubquery(
            IndexNameAndDescPair[] tables,
            int subqueryNum,
            string tableLookupStrategy)
            : base(tables)
        {
            this.subqueryNum = subqueryNum;
            this.tableLookupStrategy = tableLookupStrategy;
        }

        public int SubqueryNum => subqueryNum;

        public string TableLookupStrategy => tableLookupStrategy;
    }
} // end of namespace
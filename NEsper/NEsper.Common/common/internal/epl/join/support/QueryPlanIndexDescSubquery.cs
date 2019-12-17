///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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

        public int SubqueryNum {
            get => subqueryNum;
        }

        public string TableLookupStrategy {
            get => tableLookupStrategy;
        }
    }
} // end of namespace
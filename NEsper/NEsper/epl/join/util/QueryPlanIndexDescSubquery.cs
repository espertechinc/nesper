///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.join.util
{
    public class QueryPlanIndexDescSubquery : QueryPlanIndexDescBase
    {
        public QueryPlanIndexDescSubquery(IndexNameAndDescPair[] tables, int subqueryNum, String tableLookupStrategy)
            : base(tables)
        {
            SubqueryNum = subqueryNum;
            TableLookupStrategy = tableLookupStrategy;
        }

        public int SubqueryNum { get; private set; }
        public string TableLookupStrategy { get; private set; }
    }
}
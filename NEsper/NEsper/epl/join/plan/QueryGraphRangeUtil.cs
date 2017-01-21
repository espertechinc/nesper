///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Property lists stored as a value for each stream-to-stream relationship, for use by 
    /// <see cref="com.espertech.esper.epl.join.plan.QueryGraph" />
    /// </summary>
    public class QueryGraphRangeUtil
    {
        private static readonly IDictionary<MultiKeyUntyped, QueryGraphRangeConsolidateDesc> opsTable =
            new Dictionary<MultiKeyUntyped, QueryGraphRangeConsolidateDesc>();

        static QueryGraphRangeUtil()
        {
            Add(QueryGraphRangeEnum.LESS_OR_EQUAL, QueryGraphRangeEnum.GREATER_OR_EQUAL, QueryGraphRangeEnum.RANGE_CLOSED);
            Add(QueryGraphRangeEnum.LESS, QueryGraphRangeEnum.GREATER, QueryGraphRangeEnum.RANGE_OPEN);
            Add(QueryGraphRangeEnum.LESS_OR_EQUAL, QueryGraphRangeEnum.GREATER, QueryGraphRangeEnum.RANGE_HALF_CLOSED);
            Add(QueryGraphRangeEnum.LESS, QueryGraphRangeEnum.GREATER_OR_EQUAL, QueryGraphRangeEnum.RANGE_HALF_OPEN);
        }

        private static void Add(QueryGraphRangeEnum opOne, QueryGraphRangeEnum opTwo, QueryGraphRangeEnum range)
        {
            MultiKeyUntyped keyOne = GetKey(opOne, opTwo);
            opsTable[keyOne] = new QueryGraphRangeConsolidateDesc(range, false);
            MultiKeyUntyped keyRev = GetKey(opTwo, opOne);
            opsTable[keyRev] = new QueryGraphRangeConsolidateDesc(range, true);
        }

        private static MultiKeyUntyped GetKey(QueryGraphRangeEnum op1, QueryGraphRangeEnum op2)
        {
            return new MultiKeyUntyped(new Object[] { op1, op2 });
        }

        public static QueryGraphRangeConsolidateDesc GetCanConsolidate(QueryGraphRangeEnum op1, QueryGraphRangeEnum op2)
        {
            return opsTable.Get(GetKey(op1, op2));
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    /// <summary>
    ///     Property lists stored as a value for each stream-to-stream relationship, for use by
    ///     <seealso cref="QueryGraphForge" />.
    /// </summary>
    public class QueryGraphRangeUtil
    {
        private static readonly IDictionary<UniformPair<QueryGraphRangeEnum>, QueryGraphRangeConsolidateDesc> OPS_TABLE =
            new Dictionary<UniformPair<QueryGraphRangeEnum>, QueryGraphRangeConsolidateDesc>();

        static QueryGraphRangeUtil()
        {
            Add(
                QueryGraphRangeEnum.LESS_OR_EQUAL,
                QueryGraphRangeEnum.GREATER_OR_EQUAL,
                QueryGraphRangeEnum.RANGE_CLOSED);
            Add(QueryGraphRangeEnum.LESS, QueryGraphRangeEnum.GREATER, QueryGraphRangeEnum.RANGE_OPEN);
            Add(QueryGraphRangeEnum.LESS_OR_EQUAL, QueryGraphRangeEnum.GREATER, QueryGraphRangeEnum.RANGE_HALF_CLOSED);
            Add(QueryGraphRangeEnum.LESS, QueryGraphRangeEnum.GREATER_OR_EQUAL, QueryGraphRangeEnum.RANGE_HALF_OPEN);
        }

        private static void Add(
            QueryGraphRangeEnum opOne,
            QueryGraphRangeEnum opTwo,
            QueryGraphRangeEnum range)
        {
            var keyOne = GetKey(opOne, opTwo);
            OPS_TABLE.Put(keyOne, new QueryGraphRangeConsolidateDesc(range, false));
            var keyRev = GetKey(opTwo, opOne);
            OPS_TABLE.Put(keyRev, new QueryGraphRangeConsolidateDesc(range, true));
        }

        private static UniformPair<QueryGraphRangeEnum> GetKey(
            QueryGraphRangeEnum op1,
            QueryGraphRangeEnum op2)
        {
            return new UniformPair<QueryGraphRangeEnum>(op1, op2);
        }

        public static QueryGraphRangeConsolidateDesc GetCanConsolidate(
            QueryGraphRangeEnum op1,
            QueryGraphRangeEnum op2)
        {
            return OPS_TABLE.Get(GetKey(op1, op2));
        }
    }
} // end of namespace
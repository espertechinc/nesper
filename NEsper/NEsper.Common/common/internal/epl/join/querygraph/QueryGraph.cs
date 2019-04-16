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
    public class QueryGraph
    {
        private readonly IDictionary<UniformPair<int>, QueryGraphValue> streamJoinMap;

        public QueryGraph(IDictionary<UniformPair<int>, QueryGraphValue> streamJoinMap)
        {
            this.streamJoinMap = streamJoinMap;
        }

        public QueryGraphValue GetGraphValue(
            int streamLookup,
            int streamIndexed)
        {
            var key = new UniformPair<int>(streamLookup, streamIndexed);
            var value = streamJoinMap.Get(key);
            if (value != null) {
                return value;
            }

            return new QueryGraphValue(Collections.GetEmptyList<QueryGraphValueDesc>());
        }
    }
} // end of namespace
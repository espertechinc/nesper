///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class TableLookupKeyDesc
    {
        public TableLookupKeyDesc(
            IList<QueryGraphValueEntryHashKeyedForge> hashes,
            IList<QueryGraphValueEntryRangeForge> ranges)
        {
            Hashes = hashes;
            Ranges = ranges;
        }

        public IList<QueryGraphValueEntryHashKeyedForge> Hashes { get; }

        public IList<QueryGraphValueEntryRangeForge> Ranges { get; }

        public ExprNode[] HashExpressions {
            get {
                var nodes = new ExprNode[Hashes.Count];
                for (var i = 0; i < Hashes.Count; i++) {
                    nodes[i] = Hashes[i].KeyExpr;
                }

                return nodes;
            }
        }

        public override string ToString()
        {
            return "TableLookupKeyDesc{" +
                   "hash=" +
                   QueryGraphValueEntryHashKeyedForge.ToQueryPlan(Hashes) +
                   ", btree=" +
                   QueryGraphValueEntryRangeForge.ToQueryPlan(Ranges) +
                   '}';
        }
    }
} // end of namespace
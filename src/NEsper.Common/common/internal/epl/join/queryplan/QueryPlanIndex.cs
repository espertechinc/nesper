///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies an index to build as part of an overall query plan.
    /// </summary>
    public class QueryPlanIndex
    {
        public QueryPlanIndex(IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> items)
        {
            Items = items;
        }

        public IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> Items { get; }
    }
} // end of namespace
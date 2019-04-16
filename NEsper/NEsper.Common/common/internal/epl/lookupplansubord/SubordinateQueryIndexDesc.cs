///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryIndexDesc
    {
        public SubordinateQueryIndexDesc(
            IndexKeyInfo optionalIndexKeyInfo,
            string indexName,
            IndexMultiKey indexMultiKey,
            QueryPlanIndexItem queryPlanIndexItem)
        {
            OptionalIndexKeyInfo = optionalIndexKeyInfo;
            IndexName = indexName;
            IndexMultiKey = indexMultiKey;
            QueryPlanIndexItem = queryPlanIndexItem;
        }

        public IndexKeyInfo OptionalIndexKeyInfo { get; }

        public string IndexName { get; }

        public IndexMultiKey IndexMultiKey { get; }

        public QueryPlanIndexItem QueryPlanIndexItem { get; }
    }
} // end of namespace
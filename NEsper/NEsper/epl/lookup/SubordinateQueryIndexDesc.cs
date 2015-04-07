///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class SubordinateQueryIndexDesc
    {
        public SubordinateQueryIndexDesc(
            IndexKeyInfo indexKeyInfo,
            string indexName,
            IndexMultiKey indexMultiKey,
            QueryPlanIndexItem queryPlanIndexItem)
        {
            IndexKeyInfo = indexKeyInfo;
            IndexName = indexName;
            IndexMultiKey = indexMultiKey;
            QueryPlanIndexItem = queryPlanIndexItem;
        }

        public IndexKeyInfo IndexKeyInfo { get; private set; }

        public string IndexName { get; private set; }

        public IndexMultiKey IndexMultiKey { get; private set; }

        public QueryPlanIndexItem QueryPlanIndexItem { get; private set; }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

        public IndexKeyInfo OptionalIndexKeyInfo { get; private set; }

        public string IndexName { get; private set; }

        public IndexMultiKey IndexMultiKey { get; private set; }

        public QueryPlanIndexItem QueryPlanIndexItem { get; private set; }
    }
}
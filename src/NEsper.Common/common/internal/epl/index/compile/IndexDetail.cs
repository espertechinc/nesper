///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
    public class IndexDetail
    {
        private readonly IndexMultiKey indexMultiKey;
        private readonly QueryPlanIndexItem queryPlanIndexItem;

        public IndexDetail(
            IndexMultiKey indexMultiKey,
            QueryPlanIndexItem queryPlanIndexItem)
        {
            this.indexMultiKey = indexMultiKey;
            this.queryPlanIndexItem = queryPlanIndexItem;
        }

        public IndexMultiKey IndexMultiKey {
            get => indexMultiKey;
        }

        public QueryPlanIndexItem QueryPlanIndexItem {
            get => queryPlanIndexItem;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryIndexPlan
    {
        public SubordinateQueryIndexPlan(
            QueryPlanIndexItemForge indexItem,
            IndexMultiKey indexPropKey,
            IList<StmtClassForgeableFactory> multiKeyForgeables,
            FabricCharge fabricCharge)
        {
            IndexItem = indexItem;
            IndexPropKey = indexPropKey;
            MultiKeyForgeables = multiKeyForgeables;
            FabricCharge = fabricCharge;
        }

        public QueryPlanIndexItemForge IndexItem { get; }

        public IndexMultiKey IndexPropKey { get; }

        public IList<StmtClassForgeableFactory> MultiKeyForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace
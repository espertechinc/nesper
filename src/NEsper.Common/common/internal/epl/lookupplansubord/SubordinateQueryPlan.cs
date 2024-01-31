///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlan
    {
        public SubordinateQueryPlan(
            SubordinateQueryPlanDescForge forge,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            Forge = forge;
            AdditionalForgeables = additionalForgeables;
            FabricCharge = fabricCharge;
        }

        public SubordinateQueryPlanDescForge Forge { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubqueryIndexForgeDesc
    {
        public SubqueryIndexForgeDesc(
            EventTableFactoryFactoryForge tableForge,
            SubordTableLookupStrategyFactoryForge lookupForge,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            TableForge = tableForge;
            LookupForge = lookupForge;
            AdditionalForgeables = additionalForgeables;
            FabricCharge = fabricCharge;
        }

        public EventTableFactoryFactoryForge TableForge { get; }

        public SubordTableLookupStrategyFactoryForge LookupForge { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace
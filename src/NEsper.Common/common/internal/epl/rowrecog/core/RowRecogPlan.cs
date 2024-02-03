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


namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogPlan
    {
        private readonly RowRecogDescForge forge;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;
        private readonly FabricCharge fabricCharge;

        public RowRecogPlan(
            RowRecogDescForge forge,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            this.forge = forge;
            this.additionalForgeables = additionalForgeables;
            this.fabricCharge = fabricCharge;
        }

        public RowRecogDescForge Forge => forge;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace
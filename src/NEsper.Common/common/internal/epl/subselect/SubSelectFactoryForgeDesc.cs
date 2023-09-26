///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectFactoryForgeDesc
    {
        public SubSelectFactoryForgeDesc(
            SubSelectFactoryForge subSelectFactoryForge,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            SubSelectFactoryForge = subSelectFactoryForge;
            AdditionalForgeables = additionalForgeables;
            FabricCharge = fabricCharge;
        }

        public SubSelectFactoryForge SubSelectFactoryForge { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace
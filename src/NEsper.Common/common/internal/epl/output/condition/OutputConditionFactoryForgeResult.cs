///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionFactoryForgeResult
    {
        private readonly OutputConditionFactoryForge forge;
        private readonly FabricCharge fabricCharge;

        public OutputConditionFactoryForgeResult(
            OutputConditionFactoryForge forge,
            FabricCharge fabricCharge)
        {
            this.forge = forge;
            this.fabricCharge = fabricCharge;
        }

        public OutputConditionFactoryForge Forge => forge;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace
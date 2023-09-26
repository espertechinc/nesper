///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class HistoricalEventViewableMethodForgeDesc
    {
        private readonly HistoricalEventViewableMethodForge forge;
        private readonly FabricCharge fabricCharge;

        public HistoricalEventViewableMethodForgeDesc(
            HistoricalEventViewableMethodForge forge,
            FabricCharge fabricCharge)
        {
            this.forge = forge;
            this.fabricCharge = fabricCharge;
        }

        public HistoricalEventViewableMethodForge Forge => forge;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace
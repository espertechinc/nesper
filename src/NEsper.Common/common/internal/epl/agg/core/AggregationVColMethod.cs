///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationVColMethod
    {
        public AggregationVColMethod(
            int vcol,
            AggregationForgeFactory forge)
        {
            Vcol = vcol;
            Forge = forge;
        }

        public int Vcol { get; }

        public AggregationForgeFactory Forge { get; }
    }
} // end of namespace
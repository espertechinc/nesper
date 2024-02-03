///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationVColAccess
    {
        public AggregationVColAccess(
            int vcol,
            AggregationAccessorForge accessorForge,
            int stateNumber,
            AggregationStateFactoryForge stateForge)
        {
            Vcol = vcol;
            AccessorForge = accessorForge;
            StateNumber = stateNumber;
            StateForge = stateForge;
        }

        public int Vcol { get; }

        public AggregationAccessorForge AccessorForge { get; }

        public int StateNumber { get; }

        public AggregationStateFactoryForge StateForge { get; }
    }
} // end of namespace
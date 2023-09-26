///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.multikey;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationCodegenRowDetailDesc
    {
        public AggregationCodegenRowDetailDesc(
            AggregationCodegenRowDetailStateDesc stateDesc,
            AggregationAccessorSlotPairForge[] accessAccessors,
            MultiKeyClassRef multiKeyClassRef)
        {
            StateDesc = stateDesc;
            AccessAccessors = accessAccessors;
            MultiKeyClassRef = multiKeyClassRef;
        }

        public AggregationCodegenRowDetailStateDesc StateDesc { get; }

        public AggregationAccessorSlotPairForge[] AccessAccessors { get; }

        public MultiKeyClassRef MultiKeyClassRef { get; }
    }
} // end of namespace
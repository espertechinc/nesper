///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationMultiFunctionAnalysisResult
    {
        public AggregationMultiFunctionAnalysisResult(AggregationAccessorSlotPair[] accessorPairs, AggregationStateFactory[] stateFactories)
        {
            AccessorPairs = accessorPairs;
            StateFactories = stateFactories;
        }

        public AggregationAccessorSlotPair[] AccessorPairs { get; private set; }

        public AggregationStateFactory[] StateFactories { get; private set; }
    }
}

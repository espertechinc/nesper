///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryRequirementSubquery
    {
        public AIRegistryRequirementSubquery(
            bool hasAggregation,
            bool hasPrior,
            bool hasPrev,
            LookupStrategyDesc lookupStrategyDesc)
        {
            HasAggregation = hasAggregation;
            HasPrior = hasPrior;
            HasPrev = hasPrev;
            LookupStrategyDesc = lookupStrategyDesc;
        }

        public bool HasAggregation { get; }

        public bool HasPrior { get; }

        public bool HasPrev { get; }

        public LookupStrategyDesc LookupStrategyDesc { get; }
    }
} // end of namespace
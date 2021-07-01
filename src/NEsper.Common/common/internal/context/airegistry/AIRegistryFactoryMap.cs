///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryFactoryMap : AIRegistryFactory
    {
        public static readonly AIRegistryFactoryMap INSTANCE = new AIRegistryFactoryMap();

        private AIRegistryFactoryMap()
        {
        }

        public AIRegistryPriorEvalStrategy MakePrior()
        {
            return new AIRegistryPriorEvalStrategyMap();
        }

        public AIRegistryPreviousGetterStrategy MakePrevious()
        {
            return new AIRegistryPreviousGetterStrategyMap();
        }

        public AIRegistrySubselectLookup MakeSubqueryLookup(LookupStrategyDesc lookupStrategyDesc)
        {
            return new AIRegistrySubselectLookupMap(lookupStrategyDesc);
        }

        public AIRegistryAggregation MakeAggregation()
        {
            return new AIRegistryAggregationMap();
        }

        public AIRegistryTableAccess MakeTableAccess()
        {
            return new AIRegistryTableAccessMap();
        }

        public AIRegistryRowRecogPreviousStrategy MakeRowRecogPreviousStrategy()
        {
            return new AIRegistryRowRecogPreviousStrategyMap();
        }
    }
} // end of namespace
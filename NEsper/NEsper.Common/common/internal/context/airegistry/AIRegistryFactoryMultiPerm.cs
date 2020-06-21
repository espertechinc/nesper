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
    public class AIRegistryFactoryMultiPerm : AIRegistryFactory
    {
        public static readonly AIRegistryFactoryMultiPerm INSTANCE = new AIRegistryFactoryMultiPerm();

        private AIRegistryFactoryMultiPerm()
        {
        }

        public AIRegistryPriorEvalStrategy MakePrior()
        {
            return new AIRegistryPriorEvalStrategyMultiPerm();
        }

        public AIRegistryPreviousGetterStrategy MakePrevious()
        {
            return new AIRegistryPreviousGetterStrategyMultiPerm();
        }

        public AIRegistrySubselectLookup MakeSubqueryLookup(LookupStrategyDesc lookupStrategyDesc)
        {
            return new AIRegistrySubselectLookupMultiPerm(lookupStrategyDesc);
        }

        public AIRegistryAggregation MakeAggregation()
        {
            return new AIRegistryAggregationMultiPerm();
        }

        public AIRegistryTableAccess MakeTableAccess()
        {
            return new AIRegistryTableAccessMultiPerm();
        }

        public AIRegistryRowRecogPreviousStrategy MakeRowRecogPreviousStrategy()
        {
            return new AIRegistryRowRecogPreviousStrategyMultiPerm();
        }
    }
} // end of namespace
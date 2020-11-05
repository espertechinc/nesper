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
    public class AIRegistryFactorySingle : AIRegistryFactory
    {
        public static readonly AIRegistryFactorySingle INSTANCE = new AIRegistryFactorySingle();

        private AIRegistryFactorySingle()
        {
        }

        public AIRegistryPriorEvalStrategy MakePrior()
        {
            return new AIRegistryPriorEvalStrategySingle();
        }

        public AIRegistryPreviousGetterStrategy MakePrevious()
        {
            return new AIRegistryPreviousGetterStrategySingle();
        }

        public AIRegistrySubselectLookup MakeSubqueryLookup(LookupStrategyDesc lookupStrategyDesc)
        {
            return new AIRegistrySubselectLookupSingle(lookupStrategyDesc);
        }

        public AIRegistryAggregation MakeAggregation()
        {
            return new AIRegistryAggregationSingle();
        }

        public AIRegistryTableAccess MakeTableAccess()
        {
            return new AIRegistryTableAccessSingle();
        }

        public AIRegistryRowRecogPreviousStrategy MakeRowRecogPreviousStrategy()
        {
            return new AIRegistryRowRecogPreviousStrategySingle();
        }
    }
} // end of namespace
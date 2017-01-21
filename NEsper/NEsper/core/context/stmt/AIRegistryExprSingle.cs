///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryExprSingle : AIRegistryExprBase
    {
        public override AIRegistrySubselect AllocateAIRegistrySubselect()
        {
            return new AIRegistrySubselectSingle();
        }

        public override AIRegistryPrevious AllocateAIRegistryPrevious()
        {
            return new AIRegistryPreviousSingle();
        }

        public override AIRegistryPrior AllocateAIRegistryPrior()
        {
            return new AIRegistryPriorSingle();
        }

        public override AIRegistryAggregation AllocateAIRegistrySubselectAggregation()
        {
            return new AIRegistryAggregationSingle();
        }

        public override AIRegistryMatchRecognizePrevious AllocateAIRegistryMatchRecognizePrevious()
        {
            return new AIRegistryMatchRecognizePreviousSingle();
        }

        public override AIRegistryTableAccess AllocateAIRegistryTableAccess()
        {
            return new AIRegistryTableAccessSingle();
        }
    }
}
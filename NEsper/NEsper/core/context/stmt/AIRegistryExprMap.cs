///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryExprMap : AIRegistryExprBase
    {
        public override AIRegistrySubselect AllocateAIRegistrySubselect()
        {
            return new AIRegistrySubselectMap();
        }

        public override AIRegistryPrevious AllocateAIRegistryPrevious()
        {
            return new AIRegistryPreviousMap();
        }

        public override AIRegistryPrior AllocateAIRegistryPrior()
        {
            return new AIRegistryPriorMap();
        }

        public override AIRegistryAggregation AllocateAIRegistrySubselectAggregation()
        {
            return new AIRegistryAggregationMap();
        }

        public override AIRegistryMatchRecognizePrevious AllocateAIRegistryMatchRecognizePrevious()
        {
            return new AIRegistryMatchRecognizePreviousMap();
        }

        public override AIRegistryTableAccess AllocateAIRegistryTableAccess()
        {
            return new AIRegistryTableAccessMap();
        }
    }
}
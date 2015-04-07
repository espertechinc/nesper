///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryExprMultiPerm : AIRegistryExprBase
    {
        public override AIRegistrySubselect AllocateAIRegistrySubselect()
        {
            return new AIRegistrySubselectMultiPerm();
        }

        public override AIRegistryPrevious AllocateAIRegistryPrevious()
        {
            return new AIRegistryPreviousMultiPerm();
        }

        public override AIRegistryPrior AllocateAIRegistryPrior()
        {
            return new AIRegistryPriorMultiPerm();
        }

        public override AIRegistryAggregation AllocateAIRegistrySubselectAggregation()
        {
            return new AIRegistryAggregationMultiPerm();
        }

        public override AIRegistryMatchRecognizePrevious AllocateAIRegistryMatchRecognizePrevious()
        {
            return new AIRegistryMatchRecognizePreviousMultiPerm();
        }

        public override AIRegistryTableAccess AllocateAIRegistryTableAccess()
        {
            return new AIRegistryTableAccessMultiPerm();
        }
    }
}
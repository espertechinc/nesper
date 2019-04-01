///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateWMatchExprQueryPlan
    {
        public SubordinateWMatchExprQueryPlan(
            SubordWMatchExprLookupStrategyFactory factory, SubordinateQueryIndexDesc[] indexDescs)
        {
            Factory = factory;
            IndexDescs = indexDescs;
        }

        public SubordWMatchExprLookupStrategyFactory Factory { get; }

        public SubordinateQueryIndexDesc[] IndexDescs { get; }
    }
} // end of namespace
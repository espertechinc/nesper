///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.lookup
{
    public class SubordinateWMatchExprQueryPlanResult
    {
        private readonly SubordWMatchExprLookupStrategyFactory factory;
        private readonly SubordinateQueryIndexDesc[] indexDescs;
    
        public SubordinateWMatchExprQueryPlanResult(SubordWMatchExprLookupStrategyFactory factory, SubordinateQueryIndexDesc[] indexDescs) {
            this.factory = factory;
            this.indexDescs = indexDescs;
        }

        public SubordWMatchExprLookupStrategyFactory Factory => factory;

        public SubordinateQueryIndexDesc[] IndexDescs => indexDescs;
    }
}

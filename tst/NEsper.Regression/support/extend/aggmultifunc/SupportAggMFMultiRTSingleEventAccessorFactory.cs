///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTSingleEventAccessorFactory : AggregationMultiFunctionAccessorFactory
    {
        public AggregationMultiFunctionAccessor NewAccessor(AggregationMultiFunctionAccessorFactoryContext ctx)
        {
            return new SupportAggMFMultiRTSingleEventAccessor();
        }
    }
} // end of namespace
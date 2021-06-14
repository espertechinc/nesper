///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggfunc;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportLowerUpperCompareAggregationFunctionFactory : AggregationFunctionFactory
    {
        public AggregationFunction NewAggregator(AggregationFunctionFactoryContext ctx)
        {
            return new SupportLowerUpperCompareAggregationFunction();
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggfunc;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportInvalidAggregationFunction : AggregationFunction
    {
        public void Enter(object value)
        {
            throw new EPException("Sample exception");
        }

        public void Leave(object value)
        {
        }

        public object Value => null;

        public void Clear()
        {
        }
    }
} // end of namespace
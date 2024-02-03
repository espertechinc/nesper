///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportSupportBeanAggregationFunction : AggregationFunction
    {
        private int count;

        public void Enter(object value)
        {
            count++;
        }

        public void Leave(object value)
        {
            count--;
        }

        public void Clear()
        {
            count = 0;
        }

        public object Value => new SupportBean("XX", count);
    }
} // end of namespace
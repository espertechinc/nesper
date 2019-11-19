///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportLowerUpperCompareAggregationFunction : AggregationFunction
    {
        private int count;

        public static object[] LastEnterParameters { get; set; }

        public void Enter(object value)
        {
            var parameters = (object[]) value;
            LastEnterParameters = parameters;
            var lower = parameters[0].AsInt();
            var upper = parameters[1].AsInt();
            var val = parameters[2].AsInt();
            if (val >= lower && val <= upper) {
                count++;
            }
        }

        public void Leave(object value)
        {
            var parameters = (object[]) value;
            var lower = parameters[0].AsInt();
            var upper = parameters[1].AsInt();
            var val = parameters[2].AsInt();
            if (val >= lower && val <= upper) {
                count--;
            }
        }

        public void Clear()
        {
            count = 0;
        }

        public object Value => count;
    }
} // end of namespace
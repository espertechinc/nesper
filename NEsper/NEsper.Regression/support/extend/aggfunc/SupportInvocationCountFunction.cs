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
    public class SupportInvocationCountFunction : AggregationFunction
    {
        private int sum;

        public static long GetValueInvocationCount { get; private set; }

        public void Enter(object value)
        {
            var amount = value.AsInt();
            sum += amount;
        }

        public void Leave(object value)
        {
        }

        public object Value {
            get {
                GetValueInvocationCount++;
                return sum;
            }
        }

        public void Clear()
        {
        }

        public static void IncGetValueInvocationCount()
        {
            GetValueInvocationCount++;
        }

        public static void ResetGetValueInvocationCount()
        {
            GetValueInvocationCount = 0;
        }
    }
} // end of namespace